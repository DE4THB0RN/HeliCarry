namespace Entregador_Drone.Server.Modelos
{
    public class Entrega
    {
        public int Id { get; set; }
        public int DroneId { get; set; }
        public ICollection<Pedido> Pedidos { get; set; }
        private List<C_No> Rota { get; set; } = []; // caminho real no grafo
        public double DistanciaTotal { get; set; }
        public TimeSpan TempoEstimado { get; set; }
        public string Status { get; set; } = StatusEntrega.EmRota;

        public void CalcularDistancia()
        {
            DistanciaTotal = 0;
            for (int i = 1; i < Rota.Count; i++)
            {
                DistanciaTotal += Math.Sqrt(
                    Math.Pow(Rota[i].X - Rota[i - 1].X, 2) +
                    Math.Pow(Rota[i].Y - Rota[i - 1].Y, 2)
                );
            }
        }

        public List<C_No> GetRota()
        {
            return Rota;
        }

        public void SetRota(List<C_No> novaRota)
        {
            Rota = novaRota;
        }

    }
}
