import React, {useEffect, useState} from 'react';
import {ordersService} from '../api/ordersService';
import {OrderDto, OrderFilters, PageResponse} from '../api/types';

const OrdersTable: React.FC = () => {
    const [data, setData] = useState<PageResponse<OrderDto> | null>(null);
    const [filters, setFilters] = useState<OrderFilters>({page: 1, size: 10});
    const [loading, setLoading] = useState(false);

    const fetchOrders = async () => {
        setLoading(true);
        try {
            const result = await ordersService.getOrders(filters);
            setData(result);
        } catch (error) {
            console.error("Помилка завантаження:", error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchOrders();
    }, [filters.page, filters.size]); // Перезавантажуємо при зміні сторінки

    const handleFilterChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const {name, value} = e.target;
        setFilters(prev => ({...prev, [name]: value, page: 1})); // Скидаємо на 1 сторінку при зміні фільтрів
    };

    return (
        <div style={{marginTop: '30px'}}>
            <h2>Список замовлень</h2>

            {/* Панель фільтрів */}
            <div style={{
                display: 'grid',
                gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))',
                gap: '10px',
                marginBottom: '20px'
            }}>
                <input type="date" name="from_date" onChange={handleFilterChange} placeholder="Від дати"/>
                <input type="date" name="to_date" onChange={handleFilterChange} placeholder="До дати"/>
                <input type="number" name="min_total" onChange={handleFilterChange} placeholder="Мін. сума"/>
                <input type="number" name="max_total" onChange={handleFilterChange} placeholder="Макс. сума"/>
                <input type="text" name="jurisdiction" onChange={handleFilterChange} placeholder="Юрисдикція"/>
                <button onClick={fetchOrders}>Застосувати</button>
            </div>

            {loading ? <p>Завантаження...</p> : (
                <>
                    <table border={1} style={{width: '100%', borderCollapse: 'collapse'}}>
                        <thead>
                        <tr style={{backgroundColor: '#f2f2f2'}}>
                            <th>Дата</th>
                            <th>Координати</th>
                            <th>Subtotal</th>
                            <th>Tax</th>
                            <th>Total</th>
                            <th>Юрисдикції</th>
                        </tr>
                        </thead>
                        <tbody>
                        {data?.content.map(order => (
                            <tr key={order.id}>
                                <td>{new Date(order.timestamp).toLocaleString()}</td>
                                <td>{order.latitude.toFixed(4)}, {order.longitude.toFixed(4)}</td>
                                <td>${order.subtotal.toFixed(2)}</td>
                                <td>${order.tax_amount.toFixed(2)} ({order.composite_tax_rate * 100}%)</td>
                                <td><strong>${order.total_amount.toFixed(2)}</strong></td>
                                <td>{order.jurisdictions.join(', ')}</td>
                            </tr>
                        ))}
                        </tbody>
                    </table>

                    {/* Пагінація */}
                    <div style={{marginTop: '10px', display: 'flex', gap: '5px'}}>
                        <button
                            disabled={filters.page === 1}
                            onClick={() => setFilters(p => ({...p, page: (p.page || 1) - 1}))}
                        > Назад
                        </button>
                        <span>Сторінка {data?.pageNumber} з {data?.totalPages}</span>
                        <button
                            disabled={filters.page === data?.totalPages}
                            onClick={() => setFilters(p => ({...p, page: (p.page || 1) + 1}))}
                        > Вперед
                        </button>
                    </div>
                </>
            )}
        </div>
    );
};

export default OrdersTable;