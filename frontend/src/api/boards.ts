import client from "./client";
import type { BoardResponse, ColumnResponse, CardResponse } from "../types/api";

export async function getBoards(): Promise<BoardResponse[]> {
    const response = await client.get<BoardResponse[]>("/boards");
    return response.data;
}

export async function getColumns(boardId: string): Promise<ColumnResponse[]> {
    const response = await client.get<ColumnResponse[]>(`/boards/${boardId}/columns`);
    return response.data;
}

export async function getCards(boardId: string, columnId: string): Promise<CardResponse[]> {
    const repsonse = await client.get<CardResponse[]>(`/boards/${boardId}/columns/${columnId}/cards`);
    return repsonse.data;
}

export async function createBoard(name: string): Promise<BoardResponse> {
    const response = await client.post<BoardResponse>(`/boards`, { name });
    return response.data;
}

export async function createColumn(boardId: string, name: string): Promise<ColumnResponse> {
    const response = await client.post<ColumnResponse>(`/boards/${boardId}/columns`, { name });
    return response.data;
}

export async function createCard(boardId: string, columnId: string, title: string, description: string | null): Promise<CardResponse> {
    const response = await client.post<CardResponse>(`/boards/${boardId}/columns/${columnId}/cards`,
        { title, description }
    );
    return response.data;
}

export async function moveCard(boardId: string, cardId: string, targetColumnId: string, newOrder: number): Promise<CardResponse> {
    const response = await client.patch<CardResponse>(
        `/boards/${boardId}/cards/${cardId}/move`,
        { targetColumnId, newOrder }
    );
    return response.data;
}
