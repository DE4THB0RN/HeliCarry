import "leaflet/dist/leaflet.css";
import 'bootstrap/dist/css/bootstrap.min.css';
import Dashboard from "./components/Dashboard";
/*import EntregaForm from "./components/EntregaForm";*/

export default function App() {
    return (
        <div className="p-6 space-y-6">
            <h1 className="text-2xl font-bold mb-4">Simulação de Entregas com Drones</h1>
            <Dashboard />
        </div>
    );
}