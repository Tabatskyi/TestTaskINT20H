export interface CreateOrderDto {
    latitude: number;
    longitude: number;
    subtotal: number;
    timestamp?: string;
}

export interface TaxBreakdown {
    stateRate: number;
    countyRate: number;
    cityRate: number;
    specialRates: number;
    compositeRate: number;
}

export interface OrderDto {
    id: string;
    latitude: number;
    longitude: number;
    subtotal: number;
    taxAmount: number;
    totalAmount: number;
    timestamp: string;
    taxBreakdown: TaxBreakdown;
    jurisdictions: string[];
}

export interface ImportOrdersResponse {
    message: string;
    importedCount: number;
    skippedCount: number;
    skippedRows: number[];
    processingTimeMs: number;
}