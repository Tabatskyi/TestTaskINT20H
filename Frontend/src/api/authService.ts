import axiosInstance from './axiosInstance';

export interface LoginRequest {
    username: string;
    password: string;
}

export interface TokenResponse {
    token: string;
    expires_at: string;
}

export const authService = {
    login: async (data: LoginRequest): Promise<TokenResponse> => {
        const response = await axiosInstance.post<TokenResponse>('/auth/login', data);
        return response.data;
    },
};
