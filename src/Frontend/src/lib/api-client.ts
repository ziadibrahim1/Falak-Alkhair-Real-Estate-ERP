import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios";
import { useAuthStore } from "./auth-store";

/**
 * عميل HTTP مركزي لكل استدعاءات الـ API. يقرأ عنوان الـ API من متغيّر بيئة
 * (لا Hard-coding)، ويرفق Access Token تلقائيًا، ويحاول تجديده مرة واحدة عبر
 * Refresh Token عند تلقي 401 قبل إعادة تسجيل الخروج.
 */
export const apiClient = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000/api",
  headers: { "Content-Type": "application/json" },
});

apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

let isRefreshing = false;
let pendingRequests: Array<() => void> = [];

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as (InternalAxiosRequestConfig & { _retry?: boolean }) | undefined;

    if (error.response?.status !== 401 || !originalRequest || originalRequest._retry) {
      return Promise.reject(error);
    }

    const { refreshToken, setTokens, logout } = useAuthStore.getState();
    if (!refreshToken) {
      logout();
      return Promise.reject(error);
    }

    originalRequest._retry = true;

    if (isRefreshing) {
      return new Promise((resolve) => {
        pendingRequests.push(() => resolve(apiClient(originalRequest)));
      });
    }

    isRefreshing = true;
    try {
      const { data } = await axios.post(`${apiClient.defaults.baseURL}/auth/refresh-token`, { refreshToken });
      const result = data.data;
      setTokens(result.accessToken, result.refreshToken);
      pendingRequests.forEach((resolve) => resolve());
      pendingRequests = [];
      return apiClient(originalRequest);
    } catch (refreshError) {
      logout();
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  }
);
