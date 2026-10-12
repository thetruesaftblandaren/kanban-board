import React, { useState } from "react";
import { Link } from "react-router-dom";
import { updateBoard, deleteBoard } from "../api/boards";
import type { BoardResponse } from "../types/api";
import ErrorMessage from "./ErrorMessage";

interface Props {
    board: BoardResponse;
    onUpdated: (board: BoardResponse) => void;
    onDeleted: (boardId: string) => void;
}

export default function BoardListItem({ board, onUpdated, onDeleted }: Props) {
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
            <li className="rounded-lg bg-white p-3 shadow-sm">
                <form onSubmit={handleSave} className="flex gap-2">
                    <input
                        className="input"
                        value={name}
                        onChange={(e) => {
                            setName(e.target.value);
                            setActionError(null);
                        }}
                    />
                    <button type="submit" className="btn-primary">Save</button>
                    <button type="button" className="btn-secondary" onClick={handleCancel}>Cancel</button>
                </form>

                <ErrorMessage message={actionError} />
            </li>
        );
    }

    return (
        <li className="rounded-lg bg-white p-3 shadow-sm">
            <div className="flex items-center justify-between">
                <Link to={`/boards/${board.id}`} className="font-medium text-indigo-600 hover:underline">
                    {board.name}
                </Link>
                <div className="flex gap-1">
                    <button type="button" className="btn-secondary" onClick={handleEdit}>Edit</button>
                    <button type="button" className="btn-danger" onClick={handleDelete}>Delete</button>
                </div>
            </div>
            
            <ErrorMessage message={actionError} />
        </li>
    );
}
