using Entregador_Drone.Server.Modelos;

namespace Entregador_Drone.Server.Serviços.Interface
{
    public interface IPathfindingService
    {
        /// <summary>
        /// Retorna lista de nós do caminho de origemNoId até destinoNoId inclusive, ou null/empty se sem caminho.
        /// </summary>
        List<C_No> AStar(int origemNoId, int destinoNoId);
    }
}
