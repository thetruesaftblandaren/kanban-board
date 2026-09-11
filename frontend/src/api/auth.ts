import client from "./client";
import type { AuthResponse } from "../types/api";

export async function login(email: string, password: string): Promise<AuthResponse> {
    const response = await client.post<AuthResponse>("auth/login", { email, password });
    localStorage.setItem("token", response.data.token);
    return response.data;
}

export function logout() {
    localStorage.removeItem("token");
}
