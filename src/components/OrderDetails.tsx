import React, {useEffect, useState} from 'react';
import {ordersService} from '../api/ordersService';
import {OrderDto} from '../api/types';

interface Props {
    orderId: string;
    onClose: () => void;
}

const OrderDetails: React.FC<Props> = ({orderId, onClose}) => {
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
        <div style={{
            padding: '20px',
            border: '2px solid #007bff',
            borderRadius: '8px',
            backgroundColor: '#fff',
            marginTop: '20px'
        }}>
            <div style={{display: 'flex', justifyContent: 'space-between'}}>
                <h3>Деталі замовлення: {order.id}</h3>
                <button onClick={onClose}>Закрити</button>
            </div>

            <div style={{display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '20px'}}>
                <div>
                    <p><strong>Час:</strong> {new Date(order.timestamp).toLocaleString()}</p>
                    <p><strong>Координати:</strong> {order.latitude}, {order.longitude}</p>
                    <p><strong>Сума без податку:</strong> ${order.subtotal.toFixed(2)}</p>
                    <p><strong>Загальна сума:</strong> <span
                        style={{fontSize: '1.2em', color: 'green'}}>${order.total_amount.toFixed(2)}</span></p>
                </div>

                <div style={{backgroundColor: '#f9f9f9', padding: '15px', borderRadius: '5px'}}>
                    <h4>Tax Breakdown (Розбивка податків)</h4>
                    <ul style={{listStyle: 'none', padding: 0}}>
                        <li>🏛️ <strong>State Rate:</strong> {(order.breakdown.state_rate * 100).toFixed(3)}%</li>
                        <li>🏘️ <strong>County Rate:</strong> {(order.breakdown.county_rate * 100).toFixed(3)}%</li>
                        <li>🏙️ <strong>City Rate:</strong> {(order.breakdown.city_rate * 100).toFixed(3)}%</li>
                        <li>✨ <strong>Special Rates:</strong> {(order.breakdown.special_rates * 100).toFixed(3)}%</li>
                        <hr/>
                        <li>📊 <strong>Composite Rate:</strong> {(order.composite_tax_rate * 100).toFixed(4)}%</li>
                        <li>💰 <strong>Total Tax Amount:</strong> ${order.tax_amount.toFixed(2)}</li>
                    </ul>
                    <p><strong>Застосовані юрисдикції:</strong> {order.jurisdictions.join(', ')}</p>
                </div>
            </div>
        </div>
    );
};

export default OrderDetails;