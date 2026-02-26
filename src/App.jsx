import CreateOrderForm from './components/CreateOrderForm';
import CsvImport from './components/CsvImport';
import './App.css'

function App() {
    return (
        <div style={{maxWidth: '800px', margin: '0 auto', fontFamily: 'sans-serif'}}>
            <h1>BetterMe Drone Delivery Admin</h1>
            <CreateOrderForm/>
            <CsvImport/>
        </div>
    );
}

export default App
