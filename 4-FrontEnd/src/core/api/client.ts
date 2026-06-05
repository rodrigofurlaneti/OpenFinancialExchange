import axios from 'axios';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5000/api',
  headers: { 'Content-Type': 'application/json' },
  timeout: 15_000,
});

// Response interceptor — surface API error messages
apiClient.interceptors.response.use(
  res => res,
  error => {
    const message =
      error.response?.data?.description ??
      error.response?.data?.message ??
      error.message ??
      'An unexpected error occurred.';
    return Promise.reject(new Error(message));
  }
);
