import React, { useEffect, useState } from "react";
import axios from "axios";
import { type StatusEntrega, type StatusDrone } from "../types/simulacao";

interface EntregaStatusResponse {
    status: StatusEntrega;
    droneStatus: StatusDrone;
    bateria: number;
}

export default function EntregaSimulacao({ entregaId }: { entregaId: number }) {
    const [status, setStatus] = useState<StatusEntrega>("EmRota");
    const [bateria, setBateria] = useState<number>(100);
    const [droneStatus, setDroneStatus] = useState<StatusDrone>("Idle");

    const avancarPasso = async () => {
        const resp = await axios.post<EntregaStatusResponse>(`/api/entrega/passo/${entregaId}`);
        setStatus(resp.data.status);
        setBateria(resp.data.bateria);
        setDroneStatus(resp.data.droneStatus);
    };

    useEffect(() => {
        const timer = setInterval(avancarPasso, 500); // simulação acelerada
        return () => clearInterval(timer);
    }, []);

    return (
        <div className= "p-4 border rounded-lg" >
        <h2 className="text-xl font-bold" > Simulação da Entrega { entregaId } </h2>
            < p > Status da Entrega: { status } </p>
                < p > Status do Drone: { droneStatus }</p>
                    < p > Bateria: { bateria.toFixed(1) }% </p>
                        </div>
  );
}