using Entregador_Drone.Server.Modelos;
using Entregador_Drone.Server.Serviços.Interface;
using Microsoft.EntityFrameworkCore;

namespace Entregador_Drone.Server.Serviços
{
    public class AStarService : IPathfindingService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AStarService> _logger;

        private Dictionary<int, C_No>? _nodesById;
        private Dictionary<(int x, int y), C_No>? _nodesByCoord;

        public AStarService(AppDbContext db, ILogger<AStarService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public void Refresh(int? cidadeId = null)
        {
            IQueryable<C_No> q = _db.C_No.AsNoTracking();
            if (cidadeId.HasValue)
                q = q.Where(n => n.CidadeId == cidadeId.Value);

            var nodes = q.ToList();
            _nodesById = nodes.ToDictionary(n => n.Id);
            _nodesByCoord = nodes.ToDictionary(n => (n.X, n.Y));

            _logger.LogInformation("GridAStarService: cache carregado com {Count} nós", nodes.Count);
        }

        private void EnsureCacheLoaded()
        {
            if (_nodesById == null || _nodesByCoord == null)
                Refresh();
        }

        public List<C_No>? AStar(int origemNoId, int destinoNoId)
        {
            EnsureCacheLoaded();

            if (_nodesById == null || _nodesByCoord == null)
                return null;

            if (!_nodesById.TryGetValue(origemNoId, out var origem) ||
                !_nodesById.TryGetValue(destinoNoId, out var destino))
                return null;

            if (origem.IsObstaculo || destino.IsObstaculo)
                return null;

            if (origem.Id == destino.Id)
                return new List<C_No> { origem };

            var open = new PriorityQueue<int, int>();
            var cameFrom = new Dictionary<int, int>();
            var gScore = new Dictionary<int, int> { [origem.Id] = 0 };
            var fScore = new Dictionary<int, int> { [origem.Id] = Heuristic(origem, destino) };

            open.Enqueue(origem.Id, fScore[origem.Id]);
            var closed = new HashSet<int>();

            while (open.Count > 0)
            {
                var currentId = open.Dequeue();
                if (currentId == destino.Id)
                    return ReconstructPath(cameFrom, origem.Id, destino.Id);

                closed.Add(currentId);
                var current = _nodesById[currentId];

                foreach (var neighbor in GetNeighbors(current))
                {
                    if (neighbor.IsObstaculo) continue;
                    if (closed.Contains(neighbor.Id)) continue;

                    var tentativeG = gScore[currentId] + 1;
                    if (!gScore.TryGetValue(neighbor.Id, out var oldG) || tentativeG < oldG)
                    {
                        cameFrom[neighbor.Id] = currentId;
                        gScore[neighbor.Id] = tentativeG;
                        var f = tentativeG + Heuristic(neighbor, destino);
                        fScore[neighbor.Id] = f;
                        open.Enqueue(neighbor.Id, f);
                    }
                }
            }

            return null;
        }

        private static int Heuristic(C_No a, C_No b) =>
            Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

        private IEnumerable<C_No> GetNeighbors(C_No node)
        {
            var offsets = new (int dx, int dy)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
            foreach (var (dx, dy) in offsets)
            {
                var key = (node.X + dx, node.Y + dy);
                if (_nodesByCoord != null && _nodesByCoord.TryGetValue(key, out var nb))
                    yield return nb;
            }
        }

        private List<C_No> ReconstructPath(Dictionary<int, int> cameFrom, int origemId, int destinoId)
        {
            var path = new List<int> { destinoId };
            var cur = destinoId;
            while (cur != origemId)
            {
                if (!cameFrom.TryGetValue(cur, out var parent)) return new List<C_No>();
                cur = parent;
                path.Add(cur);
            }
            path.Reverse();
            return path.Select(id => _nodesById![id]).ToList();
        }
    }
}
