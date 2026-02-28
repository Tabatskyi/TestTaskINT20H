import React, {useState} from 'react';
import {ordersService} from '../api/ordersService';
import {CreateOrderDto, OrderDto} from '../api/types';

const CreateOrderForm: React.FC<{ onSuccess?: () => void }> = () => {
    const [formData, setFormData] = useState<CreateOrderDto>({
        latitude: 40.7128,
        longitude: -74.0060,
        subtotal: 0,
    });
    const [result, setResult] = useState<OrderDto | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
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
            <form onSubmit={handleSubmit} style={{display: 'flex', flexDirection: 'column', gap: '10px'}}>
                <input type="number" placeholder="Lat" value={formData.latitude}
                       onChange={e => setFormData({...formData, latitude: parseFloat(e.target.value)})} required/>
                <input type="number" placeholder="Lon" value={formData.longitude}
                       onChange={e => setFormData({...formData, longitude: parseFloat(e.target.value)})} required/>
                <input type="number" placeholder="Subtotal ($)" value={formData.subtotal}
                       onChange={e => setFormData({...formData, subtotal: parseFloat(e.target.value)})} required/>
                <button type="submit">Calculate & Create</button>
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