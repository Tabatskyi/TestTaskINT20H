import React, {useEffect, useState} from 'react';
import {ordersService} from '../api/ordersService';
import {OrderDto, OrderFilters, PageResponse} from '../api/types';
import OrderDetails from "./OrderDetails.tsx";
import Modal from './Modal';
import IconButton from '@mui/material/IconButton';
import InfoIcon from '@mui/icons-material/Info';
import Tooltip from '@mui/material/Tooltip';
import '../OrdersTable.css';

const OrdersTable: React.FC = () => {
    const [data, setData] = useState<PageResponse<OrderDto> | null>(null);
    const [filters, setFilters] = useState<OrderFilters>({page: 1, size: 10});
    const [loading, setLoading] = useState(false);
    const [selectedOrderId, setSelectedOrderId] = useState<string | null>(null);

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
    }, [filters.page, filters.size]);

    const handleFilterChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const {name, value} = e.target;
        setFilters(prev => ({...prev, [name]: value, page: 1}));
    };

    return (
        <div className="orders-section">
            <div className="section-header">
                <h2>Список замовлень</h2>
                <div className="pagination-controls mini">
                    <span>Сторінка {data?.pageNumber || 1} з {data?.totalPages || 1}</span>
                </div>
            </div>

            <div className="filters-grid">
                <div className="filter-item">
                    <label>Від дати</label>
                    <input type="date" name="from_date" onChange={handleFilterChange} />
                </div>
                <div className="filter-item">
                    <label>До дати</label>
                    <input type="date" name="to_date" onChange={handleFilterChange} />
                </div>
                <div className="filter-item">
                    <label>Мін. сума</label>
                    <input type="number" name="min_total" onChange={handleFilterChange} placeholder="0.00" />
                </div>
                <div className="filter-item">
                    <label>Макс. сума</label>
                    <input type="number" name="max_total" onChange={handleFilterChange} placeholder="9999" />
                </div>
                <div className="filter-item">
                    <label>Юрисдикція</label>
                    <input type="text" name="jurisdiction" onChange={handleFilterChange} placeholder="Назва..." />
                </div>
                <button className="apply-filters-btn" onClick={fetchOrders}>
                    Застосувати
                </button>
            </div>

            {loading ? (
                <div className="table-loader">Завантаження даних...</div>
            ) : (
                <div className="table-wrapper fade-in">
                    <table className="glass-table">
                        <thead>
                        <tr>
                            <th>Дата</th>
                            <th>Координати</th>
                            <th>Subtotal</th>
                            <th>Tax</th>
                            <th>Total</th>
                            <th>Юрисдикції</th>
                            <th className="text-center">Деталі</th>
                        </tr>
                        </thead>
                        <tbody>
                        {data?.content.map(order => (
                            <tr key={order.id}>
                                <td>{new Date(order.timestamp).toLocaleString()}</td>
                                <td>{order.latitude.toFixed(4)}, {order.longitude.toFixed(4)}</td>
                                <td>${order.subtotal.toFixed(2)}</td>
                                <td className="tax-col">
                                    ${order.tax_amount.toFixed(2)}
                                    <span className="tax-percent">({(order.composite_tax_rate * 100).toFixed(2)}%)</span>
                                </td>
                                <td><strong className="total-amount">${order.total_amount.toFixed(2)}</strong></td>
                                <td className="juris-cell">{order.jurisdictions.join(', ')}</td>
                                <td className="text-center">
                                    <Tooltip title="Переглянути деталі">
                                        <IconButton
                                            className="info-btn"
                                            onClick={() => setSelectedOrderId(order.id)}
                                        >
                                            <InfoIcon />
                                        </IconButton>
                                    </Tooltip>
                                </td>
                            </tr>
                        ))}
                        </tbody>
                    </table>

                    {/* Пагінація у футері таблиці */}
                    <div className="pagination-footer">
                        <button
                            className="page-btn prev"
                            disabled={filters.page === 1}
                            onClick={() => setFilters(p => ({...p, page: (p.page || 1) - 1}))}
                        >
                            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                                <polyline points="15 18 9 12 15 6"></polyline>
                            </svg>
                            <span>Назад</span>
                        </button>

                        <div className="page-indicator">
                            <span className="current-page">{data?.pageNumber}</span>
                            <span className="divider">/</span>
                            <span className="total-pages">{data?.totalPages}</span>
                        </div>

                        <button
                            className="page-btn next"
                            disabled={filters.page === data?.totalPages}
                            onClick={() => setFilters(p => ({...p, page: (p.page || 1) + 1}))}
                        >
                            <span>Вперед</span>
                            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                                <polyline points="9 18 15 12 9 6"></polyline>
                            </svg>
                        </button>
                    </div>
                </div>
            )}

            <Modal
                isOpen={!!selectedOrderId}
                onClose={() => setSelectedOrderId(null)}
                title={`Order Details: ${selectedOrderId?.substring(0, 8)}...`}
                wide
            >
                {selectedOrderId && <OrderDetails orderId={selectedOrderId}/>}
            </Modal>
        </div>
    );
};

export default OrdersTable;