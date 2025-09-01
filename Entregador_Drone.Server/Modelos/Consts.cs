namespace Entregador_Drone.Server.Modelos
{
    public static class StatusDrone
    {
        public const string Idle = "Idle";
        public const string EmVoo = "EmVoo";
        public const string Retornando = "Retornando";
        public const string Carregando = "Carregando";
    }

    public static class  StatusPedido
    {
        public const string Pendente = "Pendente";
        public const string EmTransporte = "EmTransporte";
        public const string Entregue = "Entregue";
        public const string Cancelado = "Cancelado";
    }

    public static class Prioridade
    {
        public const string Baixa = "Baixa";
        public const string Media = "Media";
        public const string Alta = "Alta";
    }

    public static class StatusEntrega
    {
        public const string EmRota = "EmRota";
        public const string Finalizada = "Finalizada";
        public const string Falhou = "Falhou";
    }
}
