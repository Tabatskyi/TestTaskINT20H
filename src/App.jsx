import CreateOrderForm from './components/CreateOrderForm';
import CsvImport from './components/CsvImport';
import OrdersTable from './components/OrdersTable';
import './App.css'

function App() {
    return (
        <div style={{maxWidth: '1000px', margin: '0 auto', padding: '20px', fontFamily: 'Arial, sans-serif'}}>
            <h1>BetterMe Drone Delivery Admin</h1>
            <div style={{display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '20px'}}>
                <CreateOrderForm/>
                <CsvImport/>
            </div>
            <OrdersTable/>
        </div>
    );
}

export default App
