import React, { useState } from "react";
import { useDroppable } from "@dnd-kit/core";
import type { ColumnResponse, CardResponse } from "../types/api";
import { createCard, updateColumn, deleteColumn } from "../api/boards";
import DraggableCard from "./DraggableCard";
import ErrorMessage from "./ErrorMessage";

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
                newCardDescription
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
            style={{
                border: "1px solid #ccc",
                padding: "1rem",
                minWidth: "200px",
                backgroundColor: isOver ? "#f0f8ff" : "white",
            }}
        >
            <ErrorMessage message={saveColumnError} />
            {isEditingColumn ? (
                <form onSubmit={handleSaveColumn}>
                    <input value={columnName} onChange={(e) => {
                        setColumnName(e.target.value);
                        setSaveColumnError(null);
                    }} />
                    <button type="submit">Save</button>
                    <button type="button" onClick={handleCancelEditColumn}>Cancel</button>
                </form>
            ) : (
                <div>
                    <h2 style={{ display: "inline" }}>{column.name}</h2>
                    <button type="button" onClick={handleEditColumn}>Edit</button>
                    <button type="button" onClick={handleDeleteColumn}>Delete</button>
                </div>
            )}

            <ul style={{ listStyle: "none", padding: 0 }}>
                {cards.map((card) => (
                    <DraggableCard
                        key={card.id}
                        card={card}
                        boardId={boardId}
                    />
                ))}
            </ul>

            <ErrorMessage message={createCardError} />
            <form onSubmit={handleCreateCard}>
                <input
                    value={newCardTitle}
                    onChange={(e) => {
                        setNewCardTitle(e.target.value)
                        setCreateCardError(null);
                    }}
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
