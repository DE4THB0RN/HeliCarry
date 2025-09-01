import React, { useEffect, useState } from "react";
import axios from "axios";
import { type drone, type pedido, type c_no } from "../types/simulacao";

interface CidadeResponse {
    nos: c_no[];
    drones: drone[];
    pedidos: pedido[];
}

const GRID_SIZE = 100;

const CityMap: React.FC = () => {
    const [nos, setNos] = useState<c_no[]>([]);
    const [drones, setDrones] = useState<drone[]>([]);
    const [pedidos, setPedidos] = useState<pedido[]>([]);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const resp = await axios.get<CidadeResponse>("/api/cidade/estado");
                setNos(resp.data.nos);
                setDrones(resp.data.drones);
                setPedidos(resp.data.pedidos);
            } catch (err) {
                console.error("Erro ao carregar estado da cidade", err);
            }
        };

        fetchData();
        const interval = setInterval(fetchData, 1000);
        return () => clearInterval(interval);
    }, []);

    const renderCell = (x: number, y: number) => {
        const no = nos.find((n) => n.X === x && n.Y === y);
        if (!no) return <td key={`${x}-${y}`} className="bg-light"></td>;

        if (no.IsBase) {
            console.log("Renderizando base em", x, y);
            return (
                <td
                    key={`${x}-${y}`}
                    className="text-center bg-info text-white fw-bold"
                    title="Base"
                >
                    B
                </td>
            );
        }

        if (no.IsObstaculo) {
            return (
                <td
                    key={`${x}-${y}`}
                    className="bg-danger"
                    title="Obstáculo"
                ></td>
            );
        }

        const drone = drones.find(
            (d) => d.LocalizacaoAtual?.X === x && d.LocalizacaoAtual?.Y === y
        );
        if (drone) {
            return (
                <td
                    key={`${x}-${y}`}
                    className="text-center bg-warning text-dark fw-bold"
                    title={`Drone ${drone.Id} (${drone.Status})`}
                >
                    D
                </td>
            );
        }

        const pedido = pedidos.find(
            (p) =>
                p.LocalizacaoCliente?.X === x && p.LocalizacaoCliente?.Y === y
        );
        if (pedido) {
            return (
                <td
                    key={`${x}-${y}`}
                    className="text-center bg-success text-white fw-bold"
                    title={`Pedido ${pedido.Id} (${pedido.Prioridade})`}
                >
                    P
                </td>
            );
        }

        return <td key={`${x}-${y}`} className="bg-light"></td>;
    };

    // --- Estatísticas ---
    const totalDrones = drones.length;
    const pedidosPendentes = pedidos.filter((p) => p.Status === "Pendente").length;
    const pedidosEntregues = pedidos.filter((p) => p.Status === "Entregue").length;
    const totalBases = nos.filter((n) => n.IsBase).length;

    return (
        <div className="card shadow-lg border-0 mt-4">
            <div className="card-body">
                <h4 className="card-title text-primary fw-bold mb-3">
                    🗺️ Mapa da Cidade
                </h4>

                <div className="row">
                    {/* Painel resumo */}
                    <div className="col-md-3 mb-3">
                        <div className="p-3 bg-light rounded shadow-sm">
                            <h6 className="fw-bold text-secondary">📊 Resumo</h6>
                            <ul className="list-group list-group-flush mt-2">
                                <li className="list-group-item d-flex justify-content-between">
                                    <span>Drones</span>
                                    <span className="fw-bold text-warning">{totalDrones}</span>
                                </li>
                                <li className="list-group-item d-flex justify-content-between">
                                    <span>Pedidos Pendentes</span>
                                    <span className="fw-bold text-success">{pedidosPendentes}</span>
                                </li>
                                <li className="list-group-item d-flex justify-content-between">
                                    <span>Pedidos Entregues</span>
                                    <span className="fw-bold text-primary">{pedidosEntregues}</span>
                                </li>
                                <li className="list-group-item d-flex justify-content-between">
                                    <span>Bases</span>
                                    <span className="fw-bold text-info">{totalBases}</span>
                                </li>
                            </ul>
                        </div>
                    </div>

                    {/* Grid do mapa */}
                    <div className="col-md-9">
                        <div
                            className="city-map overflow-auto border rounded"
                            style={{ maxHeight: "600px" }}
                        >
                            <table className="table table-bordered table-sm mb-0">
                                <tbody>
                                    {Array.from({ length: GRID_SIZE }).map((_, y) => (
                                        <tr key={y}>
                                            {Array.from({ length: GRID_SIZE }).map((_, x) =>
                                                renderCell(x, y)
                                            )}
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default CityMap;
