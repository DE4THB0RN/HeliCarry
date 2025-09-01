namespace Entregador_Drone.Server.Modelos
{
    
    public class Drone
    {
        public int Id { get; set; }
        public double CapacidadeMaximaKg { get; set; } = 10;
        public double AutonomiaKm { get; set; } = 50;  
        public double BateriaAtual { get; set; } = 100; 
        public double ConsumoPorKm { get; set; } = 2;   
        public double ConsumoPorSegundo { get; set; } = 0.01; 
        public C_No LocalizacaoAtual { get; set; }
        public string Status { get; set; } = StatusDrone.Idle;

        public bool PodeIniciarViagem(double distanciaNecessaria, double reservaPercent = 20)
        {
            var bateriaUsavel = Math.Max(0, BateriaAtual - reservaPercent);
            var custoViagem = distanciaNecessaria * ConsumoPorKm;
            return custoViagem <= bateriaUsavel;
        }

        // Consome a bateria com base na distância percorrida e no tempo de operação
        public void ConsumirBateria(double distancia, double tempoSegundos)
        {
            var consumoTotal = (distancia * ConsumoPorKm) + (tempoSegundos * ConsumoPorSegundo);
            BateriaAtual -= consumoTotal;
            if (BateriaAtual < 0) BateriaAtual = 0;
        }

        public void AtualizarStatus(string novoStatus) => Status = novoStatus;

        public void RetornarBase(C_No baseNo)
        {
            LocalizacaoAtual = baseNo;
            Status = StatusDrone.Carregando;
            Recarregar();
        }

        private void Recarregar()
        {
            BateriaAtual = 100;
            Status = StatusDrone.Idle;
        }
    }
}
