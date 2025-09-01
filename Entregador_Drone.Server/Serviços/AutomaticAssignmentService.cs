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
                    // Prevent overlapping runs
                    if (await _workerLock.WaitAsync(0))
                    {
                        try
                        {
                            using var scope = _sp.CreateScope();
                            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                            var path = scope.ServiceProvider.GetRequiredService<IPathfindingService>();
                            var distSvc = scope.ServiceProvider.GetRequiredService<DistanciaService>();
                            var planner = new GreedyPlanner(distSvc, path, context);

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
                .OrderByDescending(p => p.Prioridade)   // se Prioridade for string, ajuste; aqui consideramos numérica (ou map antes)
                .ThenBy(p => p.TempoCriacao)
                .ToListAsync(ct);

            if (pendentes.Count == 0) return;

            // carregar drones disponíveis (Idle)
            var drones = await context.Drone
                .Where(d => d.Status == StatusDrone.Idle)
                .Include(d => d.LocalizacaoAtual)
                .ToListAsync(ct);

            if (drones.Count == 0) return;

            // For each drone, try plan and assign
            foreach (var drone in drones)
            {
                // get snapshot of pending ids (we'll validate in transaction)
                var candidatePedidos = pendentes
                    .Where(p => p.Peso <= drone.CapacidadeMaximaKg) // simple filter
                    .ToList();

                if (candidatePedidos.Count == 0) continue;

                var plan = await planner.PlanForDroneAsync(drone, candidatePedidos, ct);

                if (plan == null || !plan.AssignedPedidos.Any()) continue;

                // Attempt to commit assignment atomically
                using var tx = await context.Database.BeginTransactionAsync(ct);
                try
                {
                    // re-fetch orders inside transaction and validate still pendentes
                    var ids = plan.AssignedPedidos.Select(p => p.Id).ToList();
                    var lockPedidos = await context.Pedido
                        .Where(p => ids.Contains(p.Id))
                        .ToListAsync(ct);

                    if (lockPedidos.Any(p => p.Status != StatusPedido.Pendente))
                    {
                        // someone else took some orders; abort this assignment
                        await tx.RollbackAsync(ct);
                        continue;
                    }

                    // create entrega
                    var entrega = new Entrega
                    {
                        DroneId = drone.Id,
                        Pedidos = lockPedidos,
                        DistanciaTotal = plan.TotalKm,
                        TempoEstimado = TimeSpan.FromHours(plan.TotalKm / Math.Max(1e-6, drone.AutonomiaKm)), // placeholder
                        Status = StatusEntrega.EmRota
                    };
                    entrega.SetRota(plan.Rota);

                    // update orders
                    lockPedidos.ForEach(p => {
                        p.Status = StatusPedido.EmTransporte;
                        p.EntregaId = entrega.Id; // note: entrega.Id generated after SaveChanges, we can set after SaveChanges
                    });

                    // update drone state
                    drone.Status = StatusDrone.Carregando;
                    if (entrega.GetRota != null && entrega.GetRota().Count > 0)
                        drone.LocalizacaoAtual = entrega.GetRota()[0];

                    // save entrega
                    context.Entrega.Add(entrega);
                    await context.SaveChangesAsync(ct);

                    // Now that entrega has Id, set pedidos.EntregaId properly
                    foreach (var p in lockPedidos) p.EntregaId = entrega.Id;
                    await context.SaveChangesAsync(ct);

                    await tx.CommitAsync(ct);
                    _logger.LogInformation("Assigned {Count} pedidos to drone {DroneId} in entrega {EntregaId}", lockPedidos.Count, drone.Id, entrega.Id);

                    // remove assigned from local pendentes list
                    pendentes.RemoveAll(p => ids.Contains(p.Id));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao efetivar entrega");
                    await tx.RollbackAsync(ct);
                }
            }
        }
    }
}
