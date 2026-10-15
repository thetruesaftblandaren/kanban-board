export type CardCategory = "Task" | "Bug" | "Feature";
export const CARD_CATEGORIES: CardCategory[] = ["Task", "Bug", "Feature"];

export interface AuthResponse {
    token: string;
    refreshToken: string;
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
    category: CardCategory;
    columnId: string;
    order: number;
    createdAt: string;
}
