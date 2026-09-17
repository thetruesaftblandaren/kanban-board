import client from "./client";
import type { BoardResponse, ColumnResponse, CardResponse } from "../types/api";

/* Boards */

export async function getBoards(): Promise<BoardResponse[]> {
    const response = await client.get<BoardResponse[]>("/boards");
    return response.data;
}

export async function getBoardById(boardId: string): Promise<BoardResponse> {
  const response = await client.get<BoardResponse>(`/boards/${boardId}`);
  return response.data;
}

export async function createBoard(name: string): Promise<BoardResponse> {
    const response = await client.post<BoardResponse>(`/boards`, { name });
    return response.data;
}

export async function updateBoard(boardId: string, name: string): Promise<BoardResponse> {
    const response = await client.put<BoardResponse>(`/boards/${boardId}`, { name });
    return response.data;
}

export async function deleteBoard(boardId: string): Promise<void> {
    const response = await client.delete(`/boards/${boardId}`)
    return response.data;
}

/* Columns */

export async function getColumns(boardId: string): Promise<ColumnResponse[]> {
    const response = await client.get<ColumnResponse[]>(`/boards/${boardId}/columns`);
    return response.data;
}

export async function createColumn(boardId: string, name: string): Promise<ColumnResponse> {
    const response = await client.post<ColumnResponse>(`/boards/${boardId}/columns`, { name });
    return response.data;
}

export async function updateColumn(boardId: string, columnId: string, name: string): Promise<ColumnResponse> {
    const response = await client.put<ColumnResponse>(`boards/${boardId}/columns/${columnId}`, { name });
    return response.data;
}

export async function deleteColumn(boardId: string, columnId: string): Promise<void> {
    const response = await client.delete(`boards/${boardId}/columns/${columnId}`);
    return response.data;
}

/* Cards */

export async function getCards(boardId: string, columnId: string): Promise<CardResponse[]> {
    const repsonse = await client.get<CardResponse[]>(`/boards/${boardId}/columns/${columnId}/cards`);
    return repsonse.data;
}

export async function createCard(boardId: string, columnId: string, title: string, description: string | null): Promise<CardResponse> {
    const response = await client.post<CardResponse>(`/boards/${boardId}/columns/${columnId}/cards`,
        { title, description }
    );
    return response.data;
}

export async function updateCard(
    boardId: string,
    columnId: string,
    cardId: string,
    title: string,
    description: string | null
): Promise<CardResponse> {
    const response = await client.put<CardResponse>(
        `/boards/${boardId}/columns/${columnId}/cards/${cardId}`,
        { title, description }
    );
    return response.data;
}

export async function deleteCard(boardId: string, columnId: string, cardId: string): Promise<void> {
    await client.delete(`/boards/${boardId}/columns/${columnId}/cards/${cardId}`);
}

export async function moveCard(boardId: string, cardId: string, targetColumnId: string, newOrder: number): Promise<CardResponse> {
    const response = await client.patch<CardResponse>(
        `/boards/${boardId}/cards/${cardId}/move`,
        { targetColumnId, newOrder }
    );
    return response.data;
}
