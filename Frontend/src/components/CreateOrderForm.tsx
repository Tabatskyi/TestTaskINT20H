import React, {useState} from 'react';
import {ordersService} from '../api/ordersService';
import {CreateOrderDto, OrderDto} from '../api/types';
import LocationMap from './LocationMap';
import '../CreateOrder.css';

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
        <div className="create-order-container">
            <div className="map-section">
                <p className="map-hint">📍 Click on the map to set delivery location</p>
                <div className="map-wrapper">
                    <LocationMap
                        latitude={formData.latitude}
                        longitude={formData.longitude}
                        interactive={true}
                        onLocationChange={handleMapClick}
                        height="240px"
                    />
                </div>
            </div>

            <form onSubmit={handleSubmit} className="order-form">
                <div className="form-grid">
                    <div className="input-group">
                        <label>Latitude</label>
                        <input
                            type="number"
                            step="any"
                            lang="en-US"
                            value={formData.latitude}
                            onChange={e => setFormData({...formData, latitude: parseFloat(e.target.value) || 0})}
                            required
                        />
                    </div>
                    <div className="input-group">
                        <label>Longitude</label>
                        <input
                            type="number"
                            step="any"
                            lang="en-US"
                            value={formData.longitude}
                            onChange={e => setFormData({...formData, longitude: parseFloat(e.target.value) || 0})}
                            required
                        />
                    </div>
                </div>

                <div className="input-group subtotal-section">
                    <label>Subtotal (USD)</label>
                    <input
                        type="number"
                        step="0.01"
                        lang="en-US"
                        placeholder="0.00"
                        value={formData.subtotal}
                        onChange={e => setFormData({...formData, subtotal: parseFloat(e.target.value) || 0})}
                        required
                    />
                </div>

                <button type="submit" className="submit-order-btn">
                    Calculate & Create Order
                </button>
            </form>

            {error && <div className="form-error">{error}</div>}

            {result && (
                <div className="result-card fade-in">
                    <h4>Order Summary</h4>
                    <div className="result-content">
                        <div className="result-row">
                            <span>Total Amount:</span>
                            <strong className="text-green">${result.total_amount.toFixed(2)}</strong>
                        </div>
                        <div className="result-row secondary">
                            <span>Tax ({(result.composite_tax_rate * 100).toFixed(2)}%):</span>
                            <span>${result.tax_amount.toFixed(2)}</span>
                        </div>
                        <div className="result-divider"></div>
                        <p className="result-jurisdictions">
                            {result.jurisdictions.join(' • ')}
                        </p>
                    </div>
                </div>
            )}
        </div>
    );
};

export default CreateOrderForm;