import { useState } from "react";
import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { CARD_CATEGORIES, type CardCategory, type CardResponse } from "../types/api";
import { updateCard, deleteCard } from "../api/boards";
import ErrorMessage from "./ErrorMessage";

const CATEGORY_STYLES: Record<CardCategory, string> = {
    Task: "bg-slate-100 text-slate-700",
    Bug: "bg-red-100 text-red-700",
    Feature: "bg-emerald-100 text-emerald-700",
};

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
    const [category, setCategory] = useState<CardCategory>("Task");
    const [cardActionError, setCardActionError] = useState<string | null>(null);

    function handleEdit() {
        setTitle(card.title);
        setDescription(card.description ?? "");
        setCategory(card.category);
        setCardActionError(null);
        setIsEditing(true);
    }

    function handleCancel() {
        setTitle(card.title);
        setDescription(card.description ?? "");
        setCategory(card.category);
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
            description.trim() ? description : null,
            category
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
            <li className="rounded-md bg-white p-3 shadow-sm">
                <ErrorMessage message={cardActionError} />
                <form onSubmit={handleSave} className="space-y-2">
                    <input className="input" value={title} onChange={(e) => {
                        setTitle(e.target.value);
                        setCardActionError(null);
                    }} />
                    <input className="input" value={description} onChange={(e) => setDescription(e.target.value)} />
                    <select className="input" value={category} onChange={(e) => setCategory(e.target.value as CardCategory)}>
                        {CARD_CATEGORIES.map((c) => <option key={c} value={c}>{c}</option>)}
                    </select>
                    <button type="submit" className="btn-primary">Save</button>
                    <button type="button" className="btn-secondary" onClick={handleCancel}>Cancel</button>
                </form>
            </li>
        )
    }

    return (
        <li
            ref={setNodeRef}
            style={{ transform: CSS.Transform.toString(transform), transition }}
            className={`cursor-grab rounded-md bg-white p-3 shadow-sm ${isDragging ? "opacity-50" : ""}`}
            {...listeners}
            {...attributes}
        >
            <span className={`inline-block rounded px-1.5 py-0.5 text-xs font-medium ${CATEGORY_STYLES[card.category]}`}>
                {card.category}
            </span>
            <p className="font-medium">{card.title}</p>
            {card.description && <p className="mt-1 text-sm text-slate-600">{card.description}</p>}
            <ErrorMessage message={cardActionError} />
            <div>
                <button
                    type="button"
                    className="btn-secondary"
                    onPointerDown={(e) => e.stopPropagation()}
                    onClick={handleEdit}
                >
                    Edit
                </button>
                <button
                    type="button"
                    className="btn-danger"
                    onPointerDown={(e) => e.stopPropagation()}
                    onClick={handleDelete}
                >
                    Delete
                </button>
            </div>
        </li>
    );
}
