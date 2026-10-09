import * as signalR from "@microsoft/signalr";
import { getAccessToken } from "./client";

let connection: signalR.HubConnection | null = null;

export function getConnection(): signalR.HubConnection {
    if (connection) return connection;

    connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5093/hubs/board", {
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
