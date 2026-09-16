import { useState } from "react";
import { useDraggable } from "@dnd-kit/core";
import type { CardResponse } from "../types/api";
import { updateCard, deleteCard } from "../api/boards";

interface Props {
    card: CardResponse;
    boardId: string;
}

export default function DraggableCard({ card, boardId }: Props) {
    const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({
        id: card.id,
    });

    const [isEditing, setIsEditing] = useState(false);
    const [title, setTitle] = useState(card.title);
    const [description, setDescription] = useState(card.description ?? "");

    const style = transform
    ? {
        transform: `translate(${transform.x}px, ${transform.y}px)`,
        opacity: isDragging ? 0.5 : 1,
      }
    : undefined;

    function handleEdit() {
        setTitle(card.title);
        setDescription(card.description ?? "");
        setIsEditing(true);
    }

    function handleCancel() {
        setTitle(card.title);
        setDescription(card.description ?? "");
        setIsEditing(false);
    }

    async function handleSave(e: React.SubmitEvent<HTMLFormElement>) {
        e.preventDefault();
        if (!title.trim()) return;

        try {
            await updateCard(
            boardId,
            card.columnId,
            card.id,
            title,
            description.trim() ? description : null
            );
            setIsEditing(false);
        } catch {
            console.error("Failed to update card");
        }
    }

    async function handleDelete() {
        try {
            await deleteCard(boardId, card.columnId, card.id);
        } catch {
            console.error("Failed to delete card");
        }
    }

    if (isEditing) {
        return (
            <li style={{ border: "1px solid #ddd", padding: "0.5rem", marginBottom: "0.5rem" }}>
                <form onSubmit={handleSave}>
                    <input value={title} onChange={(e) => setTitle(e.target.value)} />
                    <input value={description} onChange={(e) => setDescription(e.target.value)} />
                    <button type="submit">Save</button>
                    <button type="button" onClick={handleCancel}>Cancel</button>
                </form>
            </li>
        )
    }

    return (
        <li
            ref={setNodeRef}
            style={{
                border: "1px solid #ddd",
                padding: "0.5rem",
                marginBottom: "0.5rem",
                backgroundColor: "#fafafa",
                cursor: "grab",
                ...style,
            }}
            {...listeners}
            {...attributes}
        >
            <strong>{card.title}</strong>
            {card.description && <p>{card.description}</p>}
            <button
                type="button"
                onPointerDown={(e) => e.stopPropagation()}
                onClick={handleEdit}
            >
                Edit
            </button>
            <button
                type="button"
                onPointerDown={(e) => e.stopPropagation()}
                onClick={handleDelete}
            >
                Delete
            </button>
        </li>
    );
}
