import React, { useState } from "react";
import axios from "axios";
import { type pedido } from "../types/simulacao";

interface PedidoFormProps {
    onCreated?: (pedido: pedido) => void;
}

const PedidoForm: React.FC<PedidoFormProps> = ({ onCreated }) => {
    const [peso, setPeso] = useState<number>(0);
    const [prioridade, setPrioridade] = useState<"Baixa" | "Media" | "Alta">("Baixa");
    const [coordenadaX, setCoordenadaX] = useState<number>(0);
    const [coordenadaY, setCoordenadaY] = useState<number>(0);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        // ... validação do peso

        // O ID da localização do cliente
        const localizacaoClienteId = coordenadaX * 100 + coordenadaY;

        try {
            const novoPedido = {
                // Altere esta linha para enviar apenas o ID da localização
                localizacaoClienteId,
                peso,
                prioridade,
            };

            const resp = await axios.post<pedido>("/api/pedido", novoPedido);
            if (onCreated) onCreated(resp.data);

            // ... resetar o formulário
        } catch (err) {
            setError("Erro ao criar pedido");
            console.error(err);
        }
    };

    return (
        <div className="container mt-4">
            <div className="card shadow-lg border-0">
                <div className="card-body p-4">
                    <h4 className="card-title mb-3 text-primary fw-bold">
                        Criar Novo Pedido
                    </h4>

                    {error && (
                        <div className="alert alert-danger fade show" role="alert">
                            {error}
                        </div>
                    )}

                    <form onSubmit={handleSubmit} className="needs-validation" noValidate>
                        <div className="mb-3">
                            <label className="form-label">Peso (kg)</label>
                            <input
                                type="number"
                                className="form-control"
                                value={peso}
                                onChange={(e) => setPeso(parseFloat(e.target.value))}
                                required
                            />
                        </div>

                        <div className="mb-3">
                            <label className="form-label">Prioridade</label>
                            <select
                                className="form-select"
                                value={prioridade}
                                onChange={(e) =>
                                    setPrioridade(e.target.value as "Baixa" | "Media" | "Alta")
                                }
                            >
                                <option value="Baixa">Baixa</option>
                                <option value="Media">Média</option>
                                <option value="Alta">Alta</option>
                            </select>
                        </div>

                        <div className="row g-3">
                            <div className="col-md-6">
                                <label className="form-label">Coordenada X (1 a 100)</label>
                                <input
                                    type="number"
                                    className="form-control"
                                    value={coordenadaX}
                                    onChange={(e) => setCoordenadaX(parseInt(e.target.value))}
                                    min={1}
                                    max={100}
                                    required
                                />
                            </div>
                            <div className="col-md-6">
                                <label className="form-label">Coordenada Y (1 a 100)</label>
                                <input
                                    type="number"
                                    className="form-control"
                                    value={coordenadaY}
                                    onChange={(e) => setCoordenadaY(parseInt(e.target.value))}
                                    min={1}
                                    max={100}
                                    required
                                />
                            </div>
                        </div>

                        <div className="d-grid mt-4">
                            <button
                                type="submit"
                                className="btn btn-primary btn-lg shadow-sm"
                                style={{ transition: "all 0.2s ease-in-out" }}
                                onMouseOver={(e) =>
                                    (e.currentTarget.style.transform = "scale(1.05)")
                                }
                                onMouseOut={(e) =>
                                    (e.currentTarget.style.transform = "scale(1)")
                                }
                            >
                                Criar Pedido
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
};

export default PedidoForm;