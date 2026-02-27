import React, {useState} from 'react';
import {ordersService} from '../api/ordersService';
import {CreateOrderDto, OrderDto} from '../api/types';

const CreateOrderForm: React.FC = () => {
    const [formData, setFormData] = useState<CreateOrderDto>({
        latitude: 40.7128,
        longitude: -74.0060,
        subtotal: 0,
    });
    const [result, setResult] = useState<OrderDto | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);
        setResult(null);

        try {
            const data = await ordersService.createOrder(formData);
            setResult(data);
        } catch (err: any) {
            // Бекенд повертає помилку у форматі { error: "message" }
            setError(err.response?.data?.error || 'Помилка при створенні замовлення');
        }
    };

    return (
        <div style={{padding: '20px', border: '1px solid #ccc', borderRadius: '8px'}}>
            <h2>Нове замовлення (Manual)</h2>
            <form onSubmit={handleSubmit}>
                <div>
                    <label>Широта (Lat): </label>
                    <input
                        type="number" step="any"
                        value={formData.latitude}
                        onChange={e => setFormData({...formData, latitude: parseFloat(e.target.value)})}
                        required
                    />
                </div>
                <div>
                    <label>Довгота (Lon): </label>
                    <input
                        type="number" step="any"
                        value={formData.longitude}
                        onChange={e => setFormData({...formData, longitude: parseFloat(e.target.value)})}
                        required
                    />
                </div>
                <div>
                    <label>Сума ($): </label>
                    <input
                        type="number" step="0.01"
                        value={formData.subtotal}
                        onChange={e => setFormData({...formData, subtotal: parseFloat(e.target.value)})}
                        required
                    />
                </div>
                <button type="submit" style={{marginTop: '10px'}}>Розрахувати та створити</button>
            </form>

            {error && <p style={{color: 'red'}}>{error}</p>}

            {result && (
                <div style={{marginTop: '20px', backgroundColor: '#f0f9f0', padding: '10px'}}>
                    <h3>Результат розрахунку:</h3>
                    <p><strong>Total:</strong> ${result.total_amount.toFixed(2)}</p>
                    <p>
                        <strong>Tax:</strong> ${result.tax_amount.toFixed(2)} ({(result.composite_tax_rate * 100).toFixed(2)}%)
                    </p>
                    <p><strong>Jurisdictions:</strong> {result.jurisdictions.join(', ')}</p>
                </div>
            )}
        </div>
    );
};

export default CreateOrderForm;