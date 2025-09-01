import axios from "axios";
import { type SimulacaoResultado } from "../types/simulacao";

const api = axios.create({
    baseURL: "http://localhost:5000/api", // ajuste se precisar
});

export const simularDrones = async (drones: Drone[], pedidos: Pedido[]) => {
    const response = await api.post<SimulacaoResultado[]>("/drone/simular", {
        drones,
        pedidos,
    });
    return response.data;
};

export default api;
