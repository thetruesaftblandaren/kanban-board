import client from "./client";
import { stopConnection } from "./signalr";
import type { AuthResponse } from "../types/api";

function saveSession(data: AuthResponse) {
    localStorage.setItem("token", data.token);
    localStorage.setItem("refreshToken", data.refreshToken);
}

export async function login(email: string, password: string): Promise<AuthResponse> {
    const response = await client.post<AuthResponse>("auth/login", { email, password });
    saveSession(response.data);
    return response.data;
}

export async function register(email: string, password: string, displayName: string): Promise<AuthResponse> {
    const response = await client.post<AuthResponse>("/auth/register", { email, password, displayName });
    saveSession(response.data);
    return response.data;
}

export async function logout() {
    const refreshToken = localStorage.getItem("refreshToken");
    localStorage.removeItem("token");
    localStorage.removeItem("refreshToken");
    await stopConnection();

    if (refreshToken) {
        try {
            await client.post("/auth/logout", { refreshToken });
        } catch {
            
        }
    }
}
