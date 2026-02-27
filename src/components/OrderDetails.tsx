import React, {useEffect, useState} from 'react';
import {ordersService} from '../api/ordersService';
import {OrderDto} from '../api/types';

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

    if (loading) return <p>Завантаження деталей...</p>;
    if (!order) return <p>Замовлення не знайдено.</p>;

    return (
        <div style={{display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '20px'}}>
            <div>
                <p><strong>Час:</strong> {new Date(order.timestamp).toLocaleString()}</p>
                <p><strong>Координати:</strong> {(order.latitude).toFixed(4)}, {(order.longitude).toFixed(4)}</p>
                <p><strong>Сума без податку:</strong> ${order.subtotal.toFixed(2)}</p>
                <p><strong>Загальна сума:</strong> <span style={{fontSize: '1.2em', color: 'green'}}>${order.total_amount.toFixed(2)}</span></p>
            </div>

            <div style={{backgroundColor: '#f9f9f9', padding: '15px', borderRadius: '5px'}}>
                <h4 style={{marginTop: 0}}>Tax Breakdown</h4>
                <ul style={{listStyle: 'none', padding: 0, fontSize: '0.9em'}}>
                    <li>🏛️ State: {(order.breakdown.state_rate * 100).toFixed(3)}%</li>
                    <li>🏘️ County: {(order.breakdown.county_rate * 100).toFixed(3)}%</li>
                    <li>🏙️ City: {(order.breakdown.city_rate * 100).toFixed(3)}%</li>
                    <li>✨ Special: {(order.breakdown.special_rates * 100).toFixed(3)}%</li>
                    <hr/>
                    <li>📊 <strong>Composite:</strong> {(order.composite_tax_rate * 100).toFixed(3)}%</li>
                </ul>
                <p style={{fontSize: '0.85em'}}><strong>Jurisdictions:</strong> {order.jurisdictions.join(', ')}</p>
            </div>
        </div>
    );
};

export default OrderDetails;