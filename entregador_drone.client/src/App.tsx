import "bootstrap/dist/css/bootstrap.min.css";
import Dashboard from "./components/Dashboard";
import React, { useEffect, useState } from "react";

export default function App() {
    const [show, setShow] = useState(false);

    // Simula efeito fade-in ao carregar
    useEffect(() => {
        const timer = setTimeout(() => setShow(true), 100);
        return () => clearTimeout(timer);
    }, []);

    return (
        <div className="bg-dark min-vh-100 d-flex flex-column">
            <div
                className={`container-fluid flex-grow-1 py-4 transition-opacity fade ${show ? "show" : ""
                    }`}
            >
                <Dashboard />
            </div>
        </div>
    );
}
