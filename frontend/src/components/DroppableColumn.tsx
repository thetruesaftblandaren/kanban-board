import { useState } from "react";
import { useDroppable } from "@dnd-kit/core";
import type { ColumnResponse, CardResponse } from "../types/api";
import { createCard } from "../api/boards";
import DraggableCard from "./DraggableCard";

interface Props {
    column: ColumnResponse;
    cards: CardResponse[];
    boardId: string;
}

export default function DroppableColumn({
    column,
    cards,
    boardId,
}: Props) {
    const { setNodeRef, isOver } = useDroppable({ id: column.id });
    const [newCardTitle, setNewCardTitle] = useState("");
    const [newCardDescription, setNewCardDescription] = useState("");

    async function handleCreateCard(e: React.SubmitEvent<HTMLFormElement>) {
        e.preventDefault();
        if (!newCardTitle.trim()) return;

        try {
            await createCard(boardId,
                column.id,
                newCardTitle,
                newCardDescription
            );
            setNewCardTitle("");
            setNewCardDescription("");
        } catch {
            console.error("Failed to create card")
        }
    }

    return (
        <div
            ref={setNodeRef}
            style={{
                border: "1px solid #ccc",
                padding: "1rem",
                minWidth: "200px",
                backgroundColor: isOver ? "#f0f8ff" : "white",
            }}
        >
            <h2>{column.name}</h2>
            <ul style={{ listStyle: "none", padding: 0 }}>
                {cards.map((card) => (
                    <DraggableCard
                        key={card.id}
                        card={card}
                        boardId={boardId}
                    />
                ))}
            </ul>

            <form onSubmit={handleCreateCard}>
                <input
                    value={newCardTitle}
                    onChange={(e) => setNewCardTitle(e.target.value)}
                    placeholder="New card title"
                />
                <input
                    value={newCardDescription}
                    onChange={(e) => setNewCardDescription(e.target.value)}
                    placeholder="Description (optional)"
                />
                <button type="submit">Add card</button>
            </form>
        </div>
    );
}
