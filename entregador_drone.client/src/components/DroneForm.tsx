import React, { useState } from "react";
import axios from "axios";
import { type drone } from "../types/simulacao";

interface DroneFormProps {
    onCreated?: (drone: drone) => void;
}

const DroneForm: React.FC<DroneFormProps> = ({ onCreated }) => {
    const [capacidadeMaxKg, setCapacidadeMaxKg] = useState<number>(10);
    const [autonomiaKm, setAutonomiaKm] = useState<number>(50);
    const [bateriaAtual] = useState<number>(100);
    const [consumoPorKm, setConsumoPorKm] = useState<number>(2);
    const [consumoPorSegundo, setConsumoPorSegundo] = useState<number>(0.01);

    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        try {
            const novoDrone = {
                capacidadeMaxKg,
                autonomiaKm,
                bateriaAtual,
                consumoPorKm,
                consumoPorSegundo,
                status: "Idle",
            };

            const resp = await axios.post<drone>("/api/drone", novoDrone);
            if (onCreated) onCreated(resp.data);

            setError(null);
        } catch (err) {
            setError("Erro ao criar drone");
            console.error(err);
        }
    };

    return (
        <div className="container mt-4">
            <div className="card shadow-lg border-0">
                <div className="card-body p-4">
                    <h4 className="card-title mb-3 text-success fw-bold">
                        Registrar Novo Drone
                    </h4>

                    {error && (
                        <div className="alert alert-danger fade show" role="alert">
                            {error}
                        </div>
                    )}

                    <form onSubmit={handleSubmit} className="needs-validation" noValidate>
                        <div className="row g-3">
                            <div className="col-md-6">
                                <label className="form-label">Capacidade Máxima (kg)</label>
                                <input
                                    type="number"
                                    className="form-control"
                                    value={capacidadeMaxKg}
                                    onChange={(e) => setCapacidadeMaxKg(parseFloat(e.target.value))}
                                    required
                                />
                            </div>
                            <div className="col-md-6">
                                <label className="form-label">Autonomia (km)</label>
                                <input
                                    type="number"
                                    className="form-control"
                                    value={autonomiaKm}
                                    onChange={(e) => setAutonomiaKm(parseFloat(e.target.value))}
                                    required
                                />
                            </div>
                        </div>

                        <div className="row g-3 mt-2">
                            <div className="col-md-6">
                                <label className="form-label">Consumo por Km</label>
                                <input
                                    type="number"
                                    className="form-control"
                                    value={consumoPorKm}
                                    onChange={(e) => setConsumoPorKm(parseFloat(e.target.value))}
                                    required
                                />
                            </div>
                            <div className="col-md-6">
                                <label className="form-label">Consumo por Segundo</label>
                                <input
                                    type="number"
                                    className="form-control"
                                    value={consumoPorSegundo}
                                    onChange={(e) =>
                                        setConsumoPorSegundo(parseFloat(e.target.value))
                                    }
                                    required
                                />
                            </div>
                        </div>

                        <div className="d-grid mt-4">
                            <button
                                type="submit"
                                className="btn btn-success btn-lg shadow-sm"
                                style={{ transition: "all 0.2s ease-in-out" }}
                                onMouseOver={(e) =>
                                    (e.currentTarget.style.transform = "scale(1.05)")
                                }
                                onMouseOut={(e) =>
                                    (e.currentTarget.style.transform = "scale(1)")
                                }
                            >
                                Criar Drone
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
};

export default DroneForm;
