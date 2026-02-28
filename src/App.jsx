import React, {useState} from 'react';
import CreateOrderForm from './components/CreateOrderForm';
import CsvImport from './components/CsvImport';
import OrdersTable from './components/OrdersTable';
import Modal from './components/Modal';
import './App.css'

function App() {
    const [isOrderModalOpen, setOrderModalOpen] = useState(false);
    const [isImportModalOpen, setImportModalOpen] = useState(false);

    return (
        <div style={{maxWidth: '1100px', margin: '0 auto', padding: '20px', fontFamily: 'system-ui, sans-serif'}}>
            <header
                style={{display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '30px'}}>
                <h1>Drone Delivery Admin</h1>
                <div style={{display: 'flex', gap: '10px'}}>
                    <button onClick={() => setOrderModalOpen(true)} style={btnStyle}>+ New Order</button>
                    <button onClick={() => setImportModalOpen(true)} style={btnStyle}>↑ Import CSV</button>
                </div>
            </header>

            <OrdersTable/>

            <Modal isOpen={isOrderModalOpen} onClose={() => setOrderModalOpen(false)} title="Create New Order">
                <CreateOrderForm onSuccess={() => setOrderModalOpen(false)}/>
            </Modal>

            <Modal isOpen={isImportModalOpen} onClose={() => setImportModalOpen(false)} title="Import Orders">
                <CsvImport/>
            </Modal>
        </div>
    );
}

const btnStyle = {padding: '10px 16px', cursor: 'pointer', borderRadius: '4px', border: '1px solid #ccc'};

export default App;