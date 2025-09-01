namespace Entregador_Drone.Server.Serviços
{
    using Entregador_Drone.Server.Modelos;
    using Entregador_Drone.Server.Serviços.Interface;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    namespace ProjetoDroneDelivery.Services
    {
        public class PlannerResult
        {
            public List<C_No> Rota { get; set; } = new();
            public List<Pedido> AssignedPedidos { get; set; } = new();
            public double TotalKm { get; set; }
        }

        /// <summary>
        /// Greedy planner que tenta inserir pedidos na rota com menor custo incremental,
        /// respeitando capacidade e bateria (reserva).
        /// </summary>
        public class GreedyPlanner
        {
            private readonly DistanciaService _dist;
            private readonly IPathfindingService _path;
            private readonly AppDbContext _context;
            private readonly ILogger<GreedyPlanner> _logger;
            private const double RESERVA_PERCENT = 20.0;

            public GreedyPlanner(DistanciaService dist, IPathfindingService path, AppDbContext context, ILogger<GreedyPlanner> logger)
            {
                _dist = dist;
                _path = path;
                _context = context;
                _logger = logger;
            }

            public GreedyPlanner(DistanciaService dist, IPathfindingService path, AppDbContext context)
            {
                _dist = dist;
                _path = path;
                _context = context;
            }

            private double DistKm(Drone drone, C_No a, C_No b, C_No baseNo)
            {
                return _dist.GetDistanciaKm(drone, a, b, baseNo);
            }

            private int MapPriority(string s)
            {
                return s switch
                {
                    "Alta" => 3,
                    "Media" => 2,
                    _ => 1
                };
            }

            public async Task<PlannerResult?> PlanForDroneAsync(Drone drone, List<Pedido> candidatos, CancellationToken ct)
            {
                if (drone == null) return null;

                var baseNo = await _context.C_No.FirstOrDefaultAsync(n => n.IsBase, ct);
                var origem = drone.LocalizacaoAtual ?? baseNo;
                if (origem == null)
                {
                    _logger.LogWarning("Base e origem não definidos para drone {DroneId}", drone.Id);
                    return null;
                }

                // inicializa rota: origem -> base (se base diferente)
                var route = new List<C_No> { origem };
                if (baseNo != null && baseNo.Id != origem.Id) route.Add(baseNo);

                double pesoTotal = 0.0;
                var assigned = new List<Pedido>();

                var candidateList = candidatos
                    .OrderByDescending(p => MapPriority(p.Prioridade))
                    .ThenBy(p => p.TempoCriacao)
                    .ToList();

                bool insertedSomething = true;
                while (insertedSomething)
                {
                    insertedSomething = false;
                    double bestDelta = double.PositiveInfinity;
                    Pedido bestPedido = null;
                    int bestPos = -1;
                    List<C_No> bestTempRoute = null;

                    foreach (var p in candidateList.Except(assigned).ToList())
                    {
                        if (pesoTotal + p.Peso > drone.CapacidadeMaximaKg) continue;

                        var pNode = p.LocalizacaoCliente;
                        if (pNode == null) continue;

                        // quick check reachability from origem
                        var reachCheck = DistKm(drone, origem, pNode, baseNo);
                        if (double.IsNaN(reachCheck)) continue;

                        for (int i = 0; i < route.Count - 1; i++)
                        {
                            var a = route[i];
                            var b = route[i + 1];

                            var ab = DistKm(drone, a, b, baseNo);
                            var ap = DistKm(drone, a, pNode, baseNo);
                            var pb = DistKm(drone, pNode, b, baseNo);

                            if (double.IsNaN(ab) || double.IsNaN(ap) || double.IsNaN(pb)) continue;

                            var delta = ap + pb - ab;
                            if (delta >= bestDelta) continue;

                            // test route with insertion
                            var tempRoute = new List<C_No>(route);
                            tempRoute.Insert(i + 1, pNode);

                            double totalKm = 0;
                            bool ok = true;
                            for (int k = 0; k < tempRoute.Count - 1; k++)
                            {
                                var d = DistKm(drone, tempRoute[k], tempRoute[k + 1], baseNo);
                                if (double.IsNaN(d)) { ok = false; break; }
                                totalKm += d;
                            }
                            if (!ok) continue;

                            // estimate consumption
                            var consumoDist = totalKm * drone.ConsumoPorKm;
                            // tempo em segundos: totalKm / kmh * 3600; use autonomiaKm as placeholder speed if necessary
                            var tempoHoras = totalKm / Math.Max(1e-6, drone.AutonomiaKm);
                            var consumoTempo = tempoHoras * 3600.0 * drone.ConsumoPorSegundo;
                            var consumoTotal = consumoDist + consumoTempo;
                            var bateriaUsavel = Math.Max(0, drone.BateriaAtual - RESERVA_PERCENT);

                            if (consumoTotal <= bateriaUsavel)
                            {
                                bestDelta = delta;
                                bestPedido = p;
                                bestPos = i + 1;
                                bestTempRoute = tempRoute;
                            }
                        }
                    }

                    if (bestPedido != null && bestPos >= 0 && bestTempRoute != null)
                    {
                        route = bestTempRoute;
                        assigned.Add(bestPedido);
                        pesoTotal += bestPedido.Peso;
                        insertedSomething = true;
                    }
                }

                if (assigned.Count == 0) return null;

                // compute final total km
                double finalKm = 0;
                for (int i = 0; i < route.Count - 1; i++)
                {
                    var d = DistKm(drone, route[i], route[i + 1], null);
                    if (double.IsNaN(d)) { finalKm = double.PositiveInfinity; break; }
                    finalKm += d;
                }

                if (double.IsInfinity(finalKm)) return null;

                return new PlannerResult
                {
                    Rota = route,
                    AssignedPedidos = assigned,
                    TotalKm = finalKm
                };
            }
        }
    }
}
