import axios from 'axios';

const axiosInstance = axios.create({
    baseURL: 'http://localhost:5000', // Замініть на ваш порт, якщо він інший
    headers: {
        'Content-Type': 'application/json',
    },
});

axiosInstance.interceptors.response.use(
    (response) => response,
    (error) => {
        // Тут можна додати логіку сповіщень (наприклад, через toast)
        const message = error.response?.data?.error || 'Щось пішло не так';
        console.error('API Error:', message);
        return Promise.reject(error);
    }
);

export default axiosInstance;