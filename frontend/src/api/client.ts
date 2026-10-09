import axios, { type InternalAxiosRequestConfig } from "axios";
import type { AuthResponse } from "../types/api";

const API_URL = "http://localhost:5093/api";

const client = axios.create({ baseURL: API_URL });

function clearSession() {
    localStorage.removeItem("token");
    localStorage.removeItem("refreshToken");
}

async function refreshAccessToken(): Promise<string> {
    const refreshToken = localStorage.getItem("refreshToken");
    if (!refreshToken) throw new Error("No refresh token");

    const response = await axios.post<AuthResponse>(`${API_URL}/auth/refresh`, { refreshToken });
    localStorage.setItem("token", response.data.token);
    localStorage.setItem("refreshToken", response.data.refreshToken);
    return response.data.token;
}

let refreshPromise: Promise<string> | null = null;

function refreshOnce(): Promise<string> {
    refreshPromise ??= refreshAccessToken().finally(() => {
        refreshPromise = null;
    });
    return refreshPromise;
}

function isExpiringSoon(token: string): boolean {
    try {
        const base64 = token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/");
        const payload = JSON.parse(atob(base64));
        return payload.exp * 1000 < Date.now() + 30_000;
    } catch {
        return true;
    }
}

export async function getAccessToken(): Promise<string> {
    const token = localStorage.getItem("token");
    if (token && !isExpiringSoon(token)) return token;
    return refreshOnce();
}

client.interceptors.request.use((config) => {
    const token = localStorage.getItem("token");
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

client.interceptors.response.use(
    (response) => response,
    async (error) => {
        const original = error.config as (InternalAxiosRequestConfig & { _retry?: boolean }) | undefined;
        const isAuthCall = original?.url?.startsWith("/auth");

        if (error.response?.status === 401 && original && !original._retry && !isAuthCall) {
            original._retry = true;
            try {
                const newToken = await refreshOnce();
                original.headers.Authorization = `Bearer ${newToken}`;
                return client(original);
            } catch {
                clearSession();
                window.location.href = "/login";
            }
        }
        return Promise.reject(error);
    }
);

export default client;
