import * as signalR from "@microsoft/signalr";

let connection: signalR.HubConnection | null = null;

export function getConnection(): signalR.HubConnection {
    if (connection) return connection;

    const token = localStorage.getItem("token");

    connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5093/hubs/board", {
            accessTokenFactory: () => token ?? "",
        })
        .withAutomaticReconnect()
        .build();

    return connection;
}
