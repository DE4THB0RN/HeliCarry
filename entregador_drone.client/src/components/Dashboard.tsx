import React from "react";
import CityMap from "./CityMap";
import PedidoForm from "./PedidosForm";
import DroneForm from "./DroneForm";

const Dashboard: React.FC = () => {
    return (
        <div className="container py-4 bg-dark">
            <h1 className="text-2xl font-bold mb-4 text-light">HELICARRY</h1>
            <div className="mb-4">
                <DroneForm />
            </div>

            <div className="mb-4">
                <PedidoForm />
            </div>

            <CityMap />
        </div>
    );
};

export default Dashboard;
