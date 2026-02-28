import axios from 'axios';

// @ts-ignore - Vite provides import.meta.env at build time
const apiUrl: string = import.meta.env?.VITE_API_URL || '/api';

const axiosInstance = axios.create({
    baseURL: apiUrl,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Attach JWT token to every request
axiosInstance.interceptors.request.use((config) => {
    const token = localStorage.getItem('jwt_token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

axiosInstance.interceptors.response.use(
    (response) => response,
    (error) => {
        const message = error.response?.data?.error || 'Щось пішло не так';
        console.error('API Error:', message);

        // On 401, clear stored token so UI redirects to login
        if (error.response?.status === 401) {
            localStorage.removeItem('jwt_token');
            localStorage.removeItem('jwt_expires_at');
            window.dispatchEvent(new Event('auth-expired'));
        }

        return Promise.reject(error);
    }
);

export default axiosInstance;