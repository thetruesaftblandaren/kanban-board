import { useState } from "react";
import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import type { CardResponse } from "../types/api";
import { updateCard, deleteCard } from "../api/boards";
import ErrorMessage from "./ErrorMessage";

interface Props {
    card: CardResponse;
    boardId: string;
}

export default function DraggableCard({ card, boardId }: Props) {
    const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({
        id: card.id,
    });

    const [isEditing, setIsEditing] = useState(false);
    const [title, setTitle] = useState(card.title);
    const [description, setDescription] = useState(card.description ?? "");
    const [cardActionError, setCardActionError] = useState<string | null>(null);

    const style = {
        transform: CSS.Transform.toString(transform),
        transition,
        opacity: isDragging ? 0.5 : 1,
    };

    function handleEdit() {
        setTitle(card.title);
        setDescription(card.description ?? "");
        setCardActionError(null);
        setIsEditing(true);
    }

    function handleCancel() {
        setTitle(card.title);
        setDescription(card.description ?? "");
        setCardActionError(null);
        setIsEditing(false);
    }

    async function handleSave(e: React.SubmitEvent<HTMLFormElement>) {
        e.preventDefault();

        try {
            await updateCard(
            boardId,
            card.columnId,
            card.id,
            title,
            description.trim() ? description : null
            );
            setIsEditing(false);
            setCardActionError(null);
        } catch {
            setCardActionError("Failed to update card. Make sure the title isn't empty.");
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
                <ErrorMessage message={cardActionError} />
                <form onSubmit={handleSave}>
                    <input value={title} onChange={(e) => {
                        setTitle(e.target.value);
                        setCardActionError(null);
                    }} />
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
            <ErrorMessage message={cardActionError} />
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
