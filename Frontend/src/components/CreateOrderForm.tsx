import React, {useState} from 'react';
import {ordersService} from '../api/ordersService';
import {CreateOrderDto, OrderDto} from '../api/types';
import LocationMap from './LocationMap';

const CreateOrderForm: React.FC<{ onSuccess?: () => void }> = ({onSuccess}) => {
    const [formData, setFormData] = useState<CreateOrderDto>({
        latitude: 40.7128,
        longitude: -74.0060,
        subtotal: 0,
    });
    const [result, setResult] = useState<OrderDto | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleMapClick = (lat: number, lng: number) => {
        setFormData(prev => ({...prev, latitude: lat, longitude: lng}));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);
        try {
            const data = await ordersService.createOrder(formData);
            setResult(data);
            if (onSuccess) onSuccess();
        } catch (err: any) {
            setError(err.response?.data?.error || 'Помилка при створенні замовлення');
        }
    };

    return (
        <div>
            <p style={{fontSize: '0.85em', color: '#666', margin: '0 0 8px'}}>
                📍 Click on the map to set delivery location
            </p>
            <div style={{marginBottom: '14px', borderRadius: '8px', overflow: 'hidden'}}>
                <LocationMap
                    latitude={formData.latitude}
                    longitude={formData.longitude}
                    interactive={true}
                    onLocationChange={handleMapClick}
                    height="250px"
                />
            </div>

            <form onSubmit={handleSubmit} style={{display: 'flex', flexDirection: 'column', gap: '10px'}}>
                <div style={{display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '10px'}}>
                    <div>
                        <label style={{display: 'block', fontSize: '0.8em', color: '#666', marginBottom: '2px'}}>Latitude</label>
                        <input type="number" step="any" value={formData.latitude}
                               onChange={e => setFormData({...formData, latitude: parseFloat(e.target.value)})}
                               required
                               style={{width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ccc', boxSizing: 'border-box'}}/>
                    </div>
                    <div>
                        <label style={{display: 'block', fontSize: '0.8em', color: '#666', marginBottom: '2px'}}>Longitude</label>
                        <input type="number" step="any" value={formData.longitude}
                               onChange={e => setFormData({...formData, longitude: parseFloat(e.target.value)})}
                               required
                               style={{width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ccc', boxSizing: 'border-box'}}/>
                    </div>
                </div>
                <div>
                    <label style={{display: 'block', fontSize: '0.8em', color: '#666', marginBottom: '2px'}}>Subtotal ($)</label>
                    <input type="number" step="0.01" placeholder="0.00" value={formData.subtotal}
                           onChange={e => setFormData({...formData, subtotal: parseFloat(e.target.value)})}
                           required
                           style={{width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ccc', boxSizing: 'border-box'}}/>
                </div>
                <button type="submit" style={{
                    padding: '10px', borderRadius: '6px', border: 'none',
                    backgroundColor: '#1976d2', color: '#fff', fontWeight: 600, cursor: 'pointer'
                }}>
                    Calculate & Create
                </button>
            </form>

            {error && <p style={{color: '#d32f2f', marginTop: '10px'}}>{error}</p>}

            {result && (
                <div style={{marginTop: '14px', backgroundColor: '#f0f9f0', padding: '12px', borderRadius: '6px'}}>
                    <h4 style={{margin: '0 0 6px'}}>Result:</h4>
                    <p style={{margin: '2px 0'}}><strong>Total:</strong> ${result.total_amount.toFixed(2)}</p>
                    <p style={{margin: '2px 0'}}>
                        <strong>Tax:</strong> ${result.tax_amount.toFixed(2)} ({(result.composite_tax_rate * 100).toFixed(2)}%)
                    </p>
                    <p style={{margin: '2px 0'}}><strong>Jurisdictions:</strong> {result.jurisdictions.join(', ')}</p>
                </div>
            )}
        </div>
    );
};

export default CreateOrderForm;