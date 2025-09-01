using Entregador_Drone.Server.Modelos;
using Entregador_Drone.Server.Serviços.Interface;
using System.Collections.Concurrent;

namespace Entregador_Drone.Server.Serviços
{
    public class DistanciaService
    {
        private readonly IPathfindingService _path;
        private readonly ConcurrentDictionary<(int, int), double> _cache = new();

        // EDGE_KM: comprimento de cada aresta (ajuste conforme sua modelagem)
        private const double EDGE_KM = 0.1; // 100 m por passo

        public DistanciaService(IPathfindingService path)
        {
            _path = path;
        }

        /// <summary>
        /// Retorna distância em km entre nós. Usa cache simétrico (a,b)=(b,a).
        /// Retorna double.NaN se não há caminho.
        /// </summary>
        public double GetDistanciaKm(Drone drone, C_No origem, C_No destino, C_No baseNo)
        {
            if (origem == null || destino == null) return double.NaN;
            if (origem.Id == destino.Id) return 0.0;

            var key = origem.Id < destino.Id ? (origem.Id, destino.Id) : (destino.Id, origem.Id);

            if (_cache.TryGetValue(key, out var cached)) return cached;

            // calcula rota com A*
            var path = _path.AStar(origem.Id, destino.Id);
            if (path == null || path.Count == 0) return double.NaN;

            var km = (path.Count - 1) * EDGE_KM;

            // store both ways via canonical key
            _cache[key] = km;
            return km;
        }
    }
}
