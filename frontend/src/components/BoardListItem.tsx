import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { updateBoard, deleteBoard } from "../api/boards";
import type { BoardResponse } from "../types/api";
import ErrorMessage from "./ErrorMessage";

interface Props {
    board: BoardResponse;
    onUpdated: (board: BoardResponse) => void;
    onDeleted: (boardId: string) => void;
}

export default function BoardListItem({ board, onUpdated, onDeleted }: Props) {
    const navigate = useNavigate();
    const [isEditing, setIsEditing] = useState(false);
    const [name, setName] = useState(board.name);
    const [actionError, setActionError] = useState<string | null>(null);

    function handleEdit() {
        setName(board.name);
        setActionError(null);
        setIsEditing(true);
    }

    function handleCancel() {
        setName(board.name);
        setActionError(null);
        setIsEditing(false);
    }

    async function handleSave(e: React.SubmitEvent<HTMLFormElement>) {
        e.preventDefault();

        try {
            const updated = await updateBoard(board.id, name);
            onUpdated(updated);
            setIsEditing(false);
            setActionError(null);
        } catch {
            setActionError("Failed to rename board. Make sure the name isn't empty.");
        }
    }

    async function handleDelete() {
        if (!window.confirm(`Delete "${board.name} and everything in it?`)) return;

        try {
            await deleteBoard(board.id);
            onDeleted(board.id);
        } catch {
            setActionError("Failed to delete board. Only the owner can delete the board.")
        }
    }

    if (isEditing) {
        return (
            <li>
                <ErrorMessage message={actionError} />
                <form onSubmit={handleSave}>
                    <input
                        value={name}
                        onChange={(e) => {
                            setName(e.target.value);
                            setActionError(null);
                        }}
                    />
                    <button type="submit">Save</button>
                    <button type="button" onClick={handleCancel}>Cancel</button>
                </form>
            </li>
        );
    }

    return (
        <li>
            <button type="button" onClick={() => navigate(`/boards/${board.id}`)}>
                {board.name}
            </button>
            <button type="button" onClick={handleEdit}>Edit</button>
            <button type="button" onClick={handleDelete}>Delete</button>
            <ErrorMessage message={actionError} />
        </li>
    );
}
