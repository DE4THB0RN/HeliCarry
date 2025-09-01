export type StatusDrone =
    | "Idle"
    | "Carregando"
    | "EmVoo"
    | "Entregando"
    | "Retornando"
    | "Recarregando";

export type StatusEntrega =
    | "EmRota"
    | "Concluida"
    | "Falhou";


export interface cidade {
    Id: number;
    Nos: c_no[];
}

export interface c_no {
    Id: number;
    X: number;
    Y: number;
    IsBase: boolean;
    IsObstaculo: boolean;
    CidadeId: number;
    Cidade: cidade;
}

export interface pedido {
    Id: number;
    LocalizacaoCliente: c_no;
    Peso: number;
    Prioridade?: "Baixa" | "Media" | "Alta";
    Entrega: entrega | null;
    EntregaId: number | null; 
    Status: "Pendente" | "EmRota" | "Entregue" | "Cancelado";
    TempoCriacao: string;
}

export interface entrega {
    Id: number;
    DroneId: number;
    Pedidos: pedido[];
    Rota: c_no[];
    DistanciaTotal: number;
    TempoEstimado: string;
    Status: StatusEntrega;
}

export interface drone {
    Id: number;
    CapacidadeMaximaKg: number;
    AutonomiaKm: number;
    BateriaAtual: number;
    ConsumoPorKm: number;
    ConsumoPorSegundo: number;
    LocalizacaoAtual: c_no;
    Status: StatusDrone;
}
