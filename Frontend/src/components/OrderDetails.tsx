import React, {useEffect, useState} from 'react';
import {ordersService} from '../api/ordersService';
import {OrderDto} from '../api/types';
import LocationMap from './LocationMap';

interface Props {
    orderId: string;
}

const OrderDetails: React.FC<Props> = ({orderId}) => {
    const [order, setOrder] = useState<OrderDto | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchOrder = async () => {
            try {
                const data = await ordersService.getOrderById(orderId);
                setOrder(data);
            } catch (err) {
                console.error("Помилка завантаження деталей замовлення", err);
            } finally {
                setLoading(false);
            }
        };
        fetchOrder();
    }, [orderId]);

    if (loading) return <p className="loading-text">Завантаження деталей...</p>;
    if (!order) return <p className="error-text">Замовлення не знайдено.</p>;

    return (
        <div className="order-details-wrapper">
            <div className="map-container">
                <LocationMap
                    latitude={order.latitude}
                    longitude={order.longitude}
                    height="280px"
                />
            </div>

            <div className="details-grid">
                <div className="info-section">
                    <div className="info-row">
                        <span className="label">Час:</span>
                        <span className="value">{new Date(order.timestamp).toLocaleString()}</span>
                    </div>
                    <div className="info-row">
                        <span className="label">Координати:</span>
                        <span className="value">{(order.latitude).toFixed(4)}, {(order.longitude).toFixed(4)}</span>
                    </div>
                    <div className="info-row">
                        <span className="label">Сума без податку:</span>
                        <span className="value">${order.subtotal.toFixed(2)}</span>
                    </div>
                    <div className="info-row total-row">
                        <span className="label">Загальна сума:</span>
                        <span className="value highlighted">${order.total_amount.toFixed(2)}</span>
                    </div>
                </div>

                <div className="tax-card">
                    <h4>Tax Breakdown</h4>
                    <ul className="tax-list">
                        <li><span>🏛️ State:</span> <strong>{(order.breakdown.state_rate * 100).toFixed(3)}%</strong></li>
                        <li><span>🏘️ County:</span> <strong>{(order.breakdown.county_rate * 100).toFixed(3)}%</strong></li>
                        <li><span>🏙️ City:</span> <strong>{(order.breakdown.city_rate * 100).toFixed(3)}%</strong></li>
                        <li><span>✨ Special:</span> <strong>{(order.breakdown.special_rates * 100).toFixed(3)}%</strong></li>
                    </ul>
                    <div className="tax-divider"></div>
                    <div className="composite-row">
                        <span>📊 Composite:</span>
                        <strong>{(order.composite_tax_rate * 100).toFixed(3)}%</strong>
                    </div>
                    <p className="jurisdictions">
                        <strong>Jurisdictions:</strong> {order.jurisdictions.join(', ')}
                    </p>
                </div>
            </div>
        </div>
    );
};

export default OrderDetails;