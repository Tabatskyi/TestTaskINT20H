export interface CreateOrderDto {
    latitude: number;
    longitude: number;
    subtotal: number;
    timestamp?: string;
}

export interface TaxBreakdown {
    state_rate: number;
    county_rate: number;
    city_rate: number;
    special_rates: number;
    composite_rate: number;
}

export interface OrderDto {
    id: string;
    latitude: number;
    longitude: number;
    subtotal: number;
    tax_amount: number;
    total_amount: number;
    timestamp: string;
    composite_tax_rate: number;
    breakdown: TaxBreakdown;
    jurisdictions: string[];
}

export interface ImportOrdersResponse {
    message: string;
    importedCount: number;
    skippedCount: number;
    skippedRows: number[];
    processingTimeMs: number;
}

export interface PageResponse<T> {
    size: number;
    pageNumber: number;
    totalPages: number;
    content: T[];
}

export interface OrderFilters {
    from_date?: string;
    to_date?: string;
    min_total?: number;
    max_total?: number;
    jurisdiction?: string;
    page?: number;
    size?: number;
}