import axios from 'axios';

export const apiClient = axios.create({
    baseURL: 'http://localhost:5180/api', // Ajustar al puerto donde corra el backend .NET
    headers: {
        'Content-Type': 'application/json',
    },
});

apiClient.interceptors.request.use(async (config) => {
    // Si no es la ruta de login, agregamos el token
    if (config.url !== '/auth/login') {
        let token = localStorage.getItem('jwt_token');
        if (!token) {
            // Auto-login silencioso para propósitos de la prueba técnica
            try {
                const res = await axios.post('http://localhost:5180/api/auth/login', { email: 'admin_supervisor@domain.com' });
                token = res.data.token;
                localStorage.setItem('jwt_token', token as string);
            } catch (err) {
                console.error('Error auto-login', err);
            }
        }
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
    }
    return config;
});

apiClient.interceptors.response.use(
    (response) => response,
    (error) => {
        // Manejo centralizado de errores
        if (error.response?.status === 401) {
            localStorage.removeItem('jwt_token'); // Forzar re-login en el próximo request
        }
        console.error('API Error:', error);
        return Promise.reject(error);
    }
);
