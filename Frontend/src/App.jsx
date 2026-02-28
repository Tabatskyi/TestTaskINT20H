import React, {useState, useEffect} from 'react';
import {AuthProvider, useAuth} from './context/AuthContext';
import LoginPage from './components/LoginPage';
import CreateOrderForm from './components/CreateOrderForm';
import CsvImport from './components/CsvImport';
import OrdersTable from './components/OrdersTable';
import Modal from './components/Modal';
import './App.css'

function AdminPanel() {
    const {logout} = useAuth();
    const [isOrderModalOpen, setOrderModalOpen] = useState(false);
    const [isImportModalOpen, setImportModalOpen] = useState(false);

    return (
        <div className="admin-container">
            <header className="admin-header">
                <div className="logo-section">
                    <span className="logo-glow">🛰️</span>
                    <h1>Drone <span>Delivery</span> Admin</h1>
                </div>

                <div className="header-actions">
                    <button className="btn-primary" onClick={() => setOrderModalOpen(true)}>
                        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><path d="M12 5v14M5 12h14"/></svg>
                        New Order
                    </button>

                    <button className="btn-secondary" onClick={() => setImportModalOpen(true)}>
                        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/></svg>
                        Import CSV
                    </button>

                    <button className="btn-logout" onClick={logout}>
                        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>
                        Logout
                    </button>
                </div>
            </header>

            <OrdersTable/>

            <Modal isOpen={isOrderModalOpen} onClose={() => setOrderModalOpen(false)} title="Create New Order" wide>
                <CreateOrderForm onSuccess={() => setOrderModalOpen(false)}/>
            </Modal>

            <Modal isOpen={isImportModalOpen} onClose={() => setImportModalOpen(false)} title="Import Orders">
                <CsvImport/>
            </Modal>
        </div>
    );
}

function AppContent() {
    const {isAuthenticated, logout} = useAuth();

    // Listen for auth-expired events from axios interceptor
    useEffect(() => {
        const handler = () => logout();
        window.addEventListener('auth-expired', handler);
        return () => window.removeEventListener('auth-expired', handler);
    }, [logout]);

    if (!isAuthenticated) {
        return <LoginPage/>;
    }

    return <AdminPanel/>;
}

function App() {
    return (
        <AuthProvider>
            <AppContent/>
        </AuthProvider>
    );
}

const btnStyle = {padding: '10px 16px', cursor: 'pointer', borderRadius: '4px', border: '1px solid #ccc'};

export default App;