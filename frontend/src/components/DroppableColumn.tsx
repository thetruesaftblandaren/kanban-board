import React, { useState } from "react";
import { useDroppable } from "@dnd-kit/core";
import type { ColumnResponse, CardResponse } from "../types/api";
import { createCard, updateColumn, deleteColumn } from "../api/boards";
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

    const [isEditingColumn, setIsEditingColumn] = useState(false);
    const [columnName, setColumnName] = useState(column.name);

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

    function handleEditColumn() {
        setColumnName(column.name);
        setIsEditingColumn(true);
    }

    function handleCancelEditColumn() {
        setColumnName(column.name);
        setIsEditingColumn(false);
    }

    async function handleSaveColumn(e: React.SubmitEvent<HTMLFormElement>) {
        e.preventDefault();
        if (!columnName.trim()) return;

        try {
            await updateColumn(boardId, column.id, columnName);
            setIsEditingColumn(false);
        } catch {
            console.error("Failed to update column");
        }
    }

    async function handleDeleteColumn() {
        try {
            await deleteColumn(boardId, column.id);
        } catch {
            console.error("Failed to delete column");
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
            {isEditingColumn ? (
                <form onSubmit={handleSaveColumn}>
                    <input value={columnName} onChange={(e) => setColumnName(e.target.value)} />
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
