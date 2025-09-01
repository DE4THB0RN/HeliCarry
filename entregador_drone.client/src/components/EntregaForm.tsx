//import React, { useEffect, useState } from "react";
//import axios from "axios";
//import { type Drone, type Pedido } from "../types/simulacao";

//export default function EntregaForm() {
//    const [drones, setDrones] = useState<Drone[]>([]);
//    const [pedidos, setPedidos] = useState<Pedido[]>([]);
//    const [droneId, setDroneId] = useState<number>(0);
//    const [pedidosSelecionados, setPedidosSelecionados] = useState<number[]>([]);

//    useEffect(() => {
//        axios.get<Drone[]>("/api/drone").then((res) => setDrones(res.data));
//        axios.get<Pedido[]>("/api/pedido").then((res) => setPedidos(res.data));
//    }, []);

//    const handleSubmit = async (e: React.FormEvent) => {
//        e.preventDefault();
//        const entrega = {
//            droneId,
//            pedidos: pedidos.filter(p => pedidosSelecionados.includes(p.id)),
//            rota: [] // backend vai calcular via A*
//        };
//        await axios.post("/api/entrega/iniciar", entrega);
//    };

//    return (
//        <form onSubmit={handleSubmit} className="p-4 border rounded mb-4">
//            <h2 className="text-lg font-bold mb-2">Criar Entrega</h2>
//            <select onChange={(e) => setDroneId(Number(e.target.value))} className="border p-1 mr-2">
//                <option value={0}>Selecione um Drone</option>
//                {drones.map((d) => (
//                    <option key={d.id} value={d.id}>{d.nome}</option>
//                ))}
//            </select>
//            <div className="mb-2">
//                <p>Pedidos:</p>
//                {pedidos.map((p) => (
//                    <label key={p.id} className="block">
//                        <input
//                            type="checkbox"
//                            value={p.id}
//                            onChange={(e) => {
//                                const id = Number(e.target.value);
//                                setPedidosSelecionados((prev) =>
//                                    prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]
//                                );
//                            }}
//                        />
//                        Pedido {p.id} - {p.peso}kg ({p.prioridade})
//                    </label>
//                ))}
//            </div>
//            <button type="submit" className="bg-purple-500 text-white px-3 py-1 rounded">
//                Criar Entrega
//            </button>
//        </form>
//    );
//}
