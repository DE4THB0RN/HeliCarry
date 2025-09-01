namespace Entregador_Drone.Server.Serviços
{
    using Entregador_Drone.Server.Modelos;
    using Entregador_Drone.Server.Serviços.Interface;
    using Entregador_Drone.Server.Serviços.ProjetoDroneDelivery.Services;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class AutomaticAssignmentService : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<AutomaticAssignmentService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);
        private readonly SemaphoreSlim _workerLock = new(1, 1);

        public AutomaticAssignmentService(IServiceProvider sp, ILogger<AutomaticAssignmentService> logger)
        {
            _sp = sp;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AutomaticAssignmentService started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Loop tick em: {time}", DateTime.Now);

                    if (await _workerLock.WaitAsync(0))
                    {
                        try
                        {
                            using var scope = _sp.CreateScope();
                            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                            _logger.LogInformation("Context criado, pedidos: {count}",
                                await context.Pedido.CountAsync(stoppingToken));

                            var path = scope.ServiceProvider.GetRequiredService<IPathfindingService>();
                            var distSvc = scope.ServiceProvider.GetRequiredService<DistanciaService>();
                            var planner = new GreedyPlanner(distSvc, path, context, _logger);

                            await RunCycleAsync(context, planner, stoppingToken);
                        }
                        finally
                        {
                            _workerLock.Release();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro no dispatcher");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task RunCycleAsync(AppDbContext context, GreedyPlanner planner, CancellationToken ct)
        {
            // carregar pedidos pendentes (ordenados por prioridade/idade)
            var pendentes = await context.Pedido
                .Where(p => p.Status == StatusPedido.Pendente)
                .Include(p => p.LocalizacaoCliente)
                .OrderByDescending(p => p.Prioridade)
                .ThenBy(p => p.TempoCriacao)
                .ToListAsync(ct);

            foreach (var p in pendentes)
            {
                p.LocalizacaoCliente ??= await context.C_No
                        .FirstOrDefaultAsync(n => n.Id == p.LocalizacaoClienteId, ct);
            }

            if (pendentes.Count == 0) return;

            // carregar drones disponíveis (Idle)
            var drones = await context.Drone
                .Where(d => d.Status == StatusDrone.Idle)
                .Include(d => d.LocalizacaoAtual)
                .ToListAsync(ct);

            if (drones.Count == 0) return;

            _logger.LogInformation("Iniciando ciclo: {Pendentes} pendentes, {Drones} drones idle", pendentes.Count, drones.Count);

            foreach (var drone in drones)
            {
                _logger.LogInformation("Drone {DroneId} status={Status} cap={Capacidade}kg bateria={Bateria}%",
                    drone.Id, drone.Status, drone.CapacidadeMaximaKg, drone.BateriaAtual);

                var candidatePedidos = pendentes
                    .Where(p => p.Peso <= drone.CapacidadeMaximaKg)
                    .ToList();

                _logger.LogInformation(" -> {Count} pedidos candidatos para drone {DroneId}", candidatePedidos.Count, drone.Id);

                var plan = await planner.PlanForDroneAsync(drone, candidatePedidos, ct);

                if (plan == null)
                {
                    _logger.LogWarning(" -> Nenhum plano encontrado para drone {DroneId}", drone.Id);
                    continue;
                }

                // Criar Entrega
                var entrega = new Entrega
                {
                    DroneId = drone.Id,
                    Pedidos = plan.AssignedPedidos,
                    DistanciaTotal = plan.TotalKm,
                    Status = StatusEntrega.EmRota
                };
                entrega.SetRota(plan.Rota);
                entrega.CalcularDistancia();

                context.Entrega.Add(entrega);

                // Atualizar pedidos
                foreach (var pedido in plan.AssignedPedidos)
                {
                    pedido.Status = StatusPedido.EmTransporte;
                    pedido.Entrega = entrega;
                }

                // Atualizar drone
                drone.Status = StatusDrone.EmVoo;

                await context.SaveChangesAsync(ct);

                // Disparar simulação assíncrona
                _ = SimularEntregaAsync(entrega.Id, ct);
            }
        }

        private async Task SimularEntregaAsync(int entregaId, CancellationToken ct)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<AutomaticAssignmentService>>();

                var entrega = await context.Entrega
                    .Include(e => e.Pedidos)
                    .FirstOrDefaultAsync(e => e.Id == entregaId, ct);

                if (entrega == null) return;

                var drone = await context.Drone
                    .Include(d => d.LocalizacaoAtual)
                    .FirstOrDefaultAsync(d => d.Id == entrega.DroneId, ct);

                if (drone == null) return;

                var rota = entrega.GetRota();
                if (rota == null || rota.Count == 0) return;

                for (int i = 1; i < rota.Count; i++)
                {
                    if (ct.IsCancellationRequested) break;

                    // Atualizar posição do drone
                    drone.LocalizacaoAtual = rota[i];
                    drone.BateriaAtual -= drone.ConsumoPorKm;
                    drone.BateriaAtual -= drone.ConsumoPorSegundo; // simplificado, pode melhorar
                    await context.SaveChangesAsync(ct);

                    logger.LogInformation("Drone {DroneId} moveu para nó {X},{Y}", drone.Id, rota[i].X, rota[i].Y);

                    await Task.Delay(1000, ct); // delay de 1s entre movimentos
                }

                // --- Marcar pedidos como entregues ---
                foreach (var p in entrega.Pedidos)
                    p.Status = StatusPedido.Entregue;

                entrega.Status = StatusEntrega.Finalizada;

                // --- Retorno: rota invertida ---
                var rotaRetorno = rota.AsEnumerable().Reverse().ToList();
                for (int i = 1; i < rotaRetorno.Count; i++)
                {
                    if (ct.IsCancellationRequested) break;

                    drone.LocalizacaoAtual = rotaRetorno[i];
                    drone.BateriaAtual -= drone.ConsumoPorKm;
                    await context.SaveChangesAsync(ct);

                    logger.LogInformation("Drone {DroneId} retornando, posição atual: {X},{Y}", drone.Id, rotaRetorno[i].X, rotaRetorno[i].Y);

                    await Task.Delay(1000, ct);
                }

                // --- Drone liberado ---
                drone.Status = StatusDrone.Idle;
                drone.BateriaAtual = 100;
                await context.SaveChangesAsync(ct);

                logger.LogInformation("Entrega {EntregaId} finalizada. Drone {DroneId} de volta à base.", entrega.Id, drone.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao simular entrega {EntregaId}", entregaId);
            }
        }
    }

}
