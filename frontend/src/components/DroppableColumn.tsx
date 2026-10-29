import React, { useState } from "react";
import { useSortable, SortableContext, verticalListSortingStrategy } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { type ColumnResponse, type CardResponse, CARD_CATEGORIES, type CardCategory } from "../types/api";
import { createCard, updateColumn, deleteColumn } from "../api/boards";
import DraggableCard from "./DraggableCard";
import ErrorMessage from "./ErrorMessage";

interface Props {
    column: ColumnResponse;
    cards: CardResponse[];
    boardId: string;
    canEdit: boolean;
}

export default function DroppableColumn({
    column,
    cards,
    boardId,
    canEdit,
}: Props) {
    const { attributes, listeners, setNodeRef, transform, transition, isOver, isDragging } = useSortable({ id: column.id, disabled: !canEdit });
    const [newCardTitle, setNewCardTitle] = useState("");
    const [newCardDescription, setNewCardDescription] = useState("");
    const [category, setCategory] = useState<CardCategory>("Task");
    const [createCardError, setCreateCardError] = useState<string | null>(null);

    const [isEditingColumn, setIsEditingColumn] = useState(false);
    const [columnName, setColumnName] = useState(column.name);
    const [saveColumnError, setSaveColumnError] = useState<string | null>(null);

    async function handleCreateCard(e: React.SubmitEvent<HTMLFormElement>) {
        e.preventDefault();

        try {
            await createCard(boardId,
                column.id,
                newCardTitle,
                newCardDescription,
                category
            );
            setNewCardTitle("");
            setNewCardDescription("");
            setCreateCardError(null);
        } catch {
            setCreateCardError("Failed to create card. Make sure the title isn't empty.");
        }
    }

    function handleEditColumn() {
        setColumnName(column.name);
        setSaveColumnError(null);
        setIsEditingColumn(true);
    }

    function handleCancelEditColumn() {
        setColumnName(column.name);
        setSaveColumnError(null);
        setIsEditingColumn(false);
    }

    async function handleSaveColumn(e: React.SubmitEvent<HTMLFormElement>) {
        e.preventDefault();

        try {
            await updateColumn(boardId, column.id, columnName);
            setIsEditingColumn(false);
            setSaveColumnError(null);
        } catch {
            setSaveColumnError("Failed to update column. Make sure the name isn't empty.");
        }
    }

    async function handleDeleteColumn() {
        try {
            await deleteColumn(boardId, column.id);
        } catch {
            setSaveColumnError("Failed to delete column");
        }
    }

    return (
        <div
            ref={setNodeRef}
            style={{ transform: CSS.Transform.toString(transform), transition }}
            className={`w-72 shrink-0 rounded-lg p-3 ${
                isOver ? "bg-indigo-50" : "bg-slate-200"
            } ${isDragging ? "opacity-50" : ""}`}
        >    
            {canEdit && (
                isEditingColumn ? (
                    <form onSubmit={handleSaveColumn} className="flex gap-1">
                        <input className="input" value={columnName} onChange={(e) => {
                            setColumnName(e.target.value);
                            setSaveColumnError(null);
                        }} />
                        <button type="submit" className="btn-primary">Save</button>
                        <button type="button" className="btn-secondary" onClick={handleCancelEditColumn}>Cancel</button>
                    </form>
                ) : (
                    <div className="flex items-center gap-2">
                        <span
                            {...attributes}
                            {...listeners}
                            className="cursor-grab select-none text-slate-400"
                        >
                            ⠿
                        </span>
                        <h2 className="flex-1 font-semibold">{column.name}</h2>
                        <button type="button" className="btn-secondary" onClick={handleEditColumn}>Edit</button>
                        <button type="button" className="btn-danger" onClick={handleDeleteColumn}>Delete</button>
                    </div>
            ))}

            <ErrorMessage message={saveColumnError} />

            <SortableContext items={cards.map((c) => c.id)} strategy={verticalListSortingStrategy}>
                <ul className="mt-3 space-y-2">
                    {cards.map((card) => (
                        <DraggableCard
                            key={card.id}
                            card={card}
                            boardId={boardId}
                            canEdit={canEdit}
                        />
                    ))}
                </ul>
            </SortableContext>
            
            <ErrorMessage message={createCardError} />

            { canEdit && (
                <form onSubmit={handleCreateCard} className="mt-3 space-y-2">
                    <input
                        className="input"
                        value={newCardTitle}
                        onChange={(e) => {
                            setNewCardTitle(e.target.value)
                            setCreateCardError(null);
                        }}
                        placeholder="New card title"
                    />
                    <input
                        className="input"
                        value={newCardDescription}
                        onChange={(e) => setNewCardDescription(e.target.value)}
                        placeholder="Description (optional)"
                    />
                    <select className="input" value={category} onChange={(e) => setCategory(e.target.value as CardCategory)}>
                        {CARD_CATEGORIES.map((c) => <option key={c} value={c}>{c}</option>)}
                    </select>
                    <button type="submit" className="btn-primary w-full">Add card</button>
                </form>
            )}

        </div>
    );
}
