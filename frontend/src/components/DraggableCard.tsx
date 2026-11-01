import { useState } from "react";
import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { CARD_CATEGORIES, type BoardMemberResponse, type CardCategory, type CardResponse } from "../types/api";
import { updateCard, deleteCard, assignCard } from "../api/boards";
import ErrorMessage from "./ErrorMessage";
import Avatar from "./Avatar";

const CATEGORY_STYLES: Record<CardCategory, string> = {
    Task: "bg-slate-100 text-slate-700",
    Bug: "bg-red-100 text-red-700",
    Feature: "bg-emerald-100 text-emerald-700",
};

interface Props {
    card: CardResponse;
    boardId: string;
    canEdit: boolean;
    members: BoardMemberResponse[];
}

export default function DraggableCard({ card, boardId, canEdit, members }: Props) {
    const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({
        id: card.id,
        disabled: !canEdit
    });

    const [isEditing, setIsEditing] = useState(false);
    const [title, setTitle] = useState(card.title);
    const [description, setDescription] = useState(card.description ?? "");
    const [category, setCategory] = useState<CardCategory>("Task");
    const [cardActionError, setCardActionError] = useState<string | null>(null);
    const assignee = members.find((m) => m.userId === card.assigneeId)

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

    async function handleAssign(assigneeId: string | null) {
        try {
            await assignCard(boardId, card.columnId, card.id , assigneeId);
            setCardActionError(null);
        } catch {
            setCardActionError("Failed to assign card.");
        }
    }

    if (isEditing) {
        return (
            <li className="rounded-md border-slate-200 bg-white p-3 shadow-sm">
                <ErrorMessage message={cardActionError} className="mb-2" />
                <form onSubmit={handleSave} className="space-y-2">
                    <input className="input" value={title} onChange={(e) => {
                        setTitle(e.target.value);
                        setCardActionError(null);
                    }} />
                    <input className="input" value={description} onChange={(e) => setDescription(e.target.value)} />
                    <select className="input" value={category} onChange={(e) => setCategory(e.target.value as CardCategory)}>
                        {CARD_CATEGORIES.map((c) => <option key={c} value={c}>{c}</option>)}
                    </select>
                    <div className="flex gap-1">
                        <button type="submit" className="btn-primary">Save</button>
                        <button type="button" className="btn-secondary" onClick={handleCancel}>Cancel</button>
                    </div>
                    
                </form>
            </li>
        )
    }

    return (
        <li
            ref={setNodeRef}
            style={{ transform: CSS.Transform.toString(transform), transition }}
            className={`group rounded-md border border-slate-200 bg-white p-3 shadow-sm ${
                canEdit ? "cursor-grab hover:shadow" : ""
            } ${isDragging ? "opacity-50" : ""}`}
            {...listeners}
            {...attributes}
        >
            <div className="flex items-center gap-2">
                <span className={`inline-block rounded px-1.5 py-0.5 text-xs font-medium ${CATEGORY_STYLES[card.category]}`}>
                    {card.category}
                </span>
                {assignee && (
                    <span
                        title={assignee.displayName}
                        className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-indigo-100 text-xs font-semibold text-indigo-700"
                    >
                        {assignee && <Avatar userId={assignee.userId} displayName={assignee.displayName} />}
                    </span>
                )}
            </div>
            
            <p className="mt-2 font-medium leading-snug">{card.title}</p>
            {card.description && <p className="mt-1 line-clamp-3 text-sm text-slate-600">{card.description}</p>}

            <ErrorMessage message={cardActionError} />

            {canEdit && (

                <div className="mt3- flex gap-1 md:opacity-0 md:group-hover:opacity-100 md:group-focus-within:opacity-100">
                    <select
                        className="mt-2 w-full rounded border border-slate-200 bg-white px-1 py-0.5 text-xs text-slate-600"
                        value={card.assigneeId ?? ""}
                        onPointerDown={(e) => e.stopPropagation()}
                        onChange={(e) => handleAssign(e.target.value || null)}
                    >
                        <option value="">Unassigned</option>
                        {members.map((m) => <option key={m.userId} value={m.userId}>{m.displayName}</option>)}
                    </select>
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
            )}

        </li>
    );
}
