import * as signalR from "@microsoft/signalr";
import { API_BASE, getAccessToken } from "./client";

let connection: signalR.HubConnection | null = null;
let startPromise: Promise<void> | null = null;

export function getConnection(): signalR.HubConnection {
    if (connection) return connection;

    connection = new signalR.HubConnectionBuilder()
        .withUrl(`${API_BASE}/hubs/board`, {
            accessTokenFactory: () => getAccessToken().catch(() => ""),
        })
        .withAutomaticReconnect()
        .build();

    return connection;
}

export async function stopConnection() {
    if (connection) {
        await connection.stop();
        connection = null;
    }
}

export async function ensureConnected(): Promise<signalR.HubConnection> {
    const conn = getConnection();

    if (conn.state === signalR.HubConnectionState.Disconnected && !startPromise) {
        startPromise = conn.start().finally(() => {
            startPromise = null;
        });
    }
    if (startPromise) await startPromise;

    return conn;
}
