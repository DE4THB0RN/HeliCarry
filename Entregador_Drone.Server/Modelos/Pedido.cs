
using System.ComponentModel.DataAnnotations.Schema;

namespace Entregador_Drone.Server.Modelos
{
    public class PedidoDto
    {
        public int LocalizacaoClienteId { get; set; } // Apenas o ID
        public double Peso { get; set; }
        public string Prioridade { get; set; }
    }

    public class Pedido
    {
        public int Id { get; set; }
        public C_No LocalizacaoCliente { get; set; }
        public double Peso { get; set; }
        public string Prioridade { get; set; }
        public int? EntregaId { get; set; }
        public Entrega? Entrega { get; set; }
        public string Status { get; set; } = StatusPedido.Pendente;
        public DateTime TempoCriacao { get; set; } = DateTime.Now;

        public bool ValidarPeso(double capacidadeDrone) =>
            Peso <= capacidadeDrone;

        public void AlterarStatus(string novoStatus)
        {
            Status = novoStatus;
        }
    }
}
