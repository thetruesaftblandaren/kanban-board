export interface AuthResponse {
    token: string;
    userId: string;
    displayName: string;
}

export interface BoardResponse {
    id: string;
    name: string;
    ownerId: string;
    createdAt: string;
}

export interface ColumnResponse {
    id: string;
    name: string;
    boardId: string;
    order: number;
}

export interface CardResponse {
    id: string;
    title: string;
    description: string | null;
    columnId: string;
    order: number;
    createdAt: string;
}
