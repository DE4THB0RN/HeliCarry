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
    id: number;
    nos: c_no[];
}

export interface c_no {
    id: number;
    x: number;
    y: number;
    isBase: boolean;
    isObstaculo: boolean;
    cidadeId: number;
    cidade: cidade;
}

export interface pedido {
    id: number;
    localizacaoCliente: c_no;
    peso: number;
    prioridade?: "Baixa" | "Media" | "Alta";
    entrega: entrega | null;
    entregaId: number | null; 
    status: "Pendente" | "EmRota" | "Entregue" | "Cancelado";
    tempoCriacao: string;
}

export interface entrega {
    id: number;
    droneId: number;
    pedidos: pedido[];
    rota: c_no[];
    distanciaTotal: number;
    tempoEstimado: string;
    status: StatusEntrega;
}

export interface drone {
    id: number;
    capacidadeMaximaKg: number;
    autonomiaKm: number;
    bateriaAtual: number;
    consumoPorKm: number;
    consumoPorSegundo: number;
    localizacaoAtual: c_no;
    status: StatusDrone;
}
