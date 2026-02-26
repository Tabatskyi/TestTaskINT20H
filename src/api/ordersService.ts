import axiosInstance from './axiosInstance';
import {OrderDto, CreateOrderDto, ImportOrdersResponse, OrderFilters, PageResponse} from './types';

export const ordersService = {
    createOrder: async (data: CreateOrderDto): Promise<OrderDto> => {
        const response = await axiosInstance.post<OrderDto>('/orders', data);
        return response.data;
    },

    importCsv: async (file: File): Promise<ImportOrdersResponse> => {
        const formData = new FormData();
        formData.append('file', file);
        const response = await axiosInstance.post<ImportOrdersResponse>('/orders/import', formData, {
            headers: {'Content-Type': 'multipart/form-data'},
        });
        return response.data;
    },

    getOrders: async (filters: OrderFilters): Promise<PageResponse<OrderDto>> => {
        const response = await axiosInstance.get<PageResponse<OrderDto>>('/orders', {
            params: filters
        });
        return response.data;
    },
};