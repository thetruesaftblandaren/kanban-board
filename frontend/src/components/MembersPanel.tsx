import { useState } from "react";
import axios from "axios";
import { inviteMember, changeMemberRole, removeMember } from "../api/boards";
import type { BoardMemberResponse, BoardRole } from "../types/api";
import ErrorMessage from "./ErrorMessage";

interface Props {
    boardId: string;
    members: BoardMemberResponse[];
    canManage: boolean;
}

function errorMessage(err: unknown, fallback: string) {
    if (axios.isAxiosError(err) && Array.isArray(err.response?.data)) {
        return err.response.data.join(" ");
    }
    return fallback;
}

export default function MembersPanel({ boardId, members, canManage }: Props) {
    const [email, setEmail] = useState("");
    const [role, setRole] = useState<BoardRole>("Editor");
    const [inviteError, setInviteError] = useState<string | null>(null);
    const [memberError, setMemberError] = useState<string | null>(null);

    async function handleInvite(e: React.SubmitEvent<HTMLFormElement>) {
        e.preventDefault();

        try {
            await inviteMember(boardId, email, role);
            setEmail("");
            setInviteError(null);
        } catch (err) {
            setInviteError(errorMessage(err, "Failed to invite member."));
        }
    }

    async function handleRoleChange(userId: string, newRole: BoardRole) {
        try {
        await changeMemberRole(boardId, userId, newRole);
        setMemberError(null);
        } catch (err) {
        setMemberError(errorMessage(err, "Failed to change role."));
        }
    }

    async function handleRemove(member: BoardMemberResponse) {
        if (!window.confirm(`Remove ${member.displayName} from this board?`)) return;
        try {
            await removeMember(boardId, member.userId);
            setMemberError(null);
        } catch (err) {
            setMemberError(errorMessage(err, "Failed to remove member."));
        }
    }

     return (
        <section className="max-w-md space-y-3 rounded-lg bg-white p-4 shadow-sm">
            <h2 className="font-semibold">Members</h2>

            <ul className="space-y-2 text-sm">
                {members.map((m) => (
                    <li key={m.userId} className="flex items-center justify-between gap-2">
                        <span>{m.displayName}</span>
                        {canManage && m.role != "Owner" ? (
                            <div className="flex items-center gap-1">
                                <select
                                    className="input w-24"
                                    value={m.role}
                                    onChange={(e) => handleRoleChange(m.userId, e.target.value as BoardRole)}
                                >
                                    <option value="Editor">Editor</option>
                                    <option value="Viewer">Viewer</option>
                                </select>
                                <button type="button" onClick={() => handleRemove(m)} className="btn-danger">
                                    Remove
                                </button>
                            </div>
                            ) : (
                                <span className="text-slate-500">{m.role}</span>
                            )}
                    </li>
                ))}
            </ul>

            <ErrorMessage message={memberError} />

            {canManage && (
                <form onSubmit={handleInvite} className="space-y-2">
                    <div className="flex gap-2">
                        <input type="email" className="input" value={email} placeholder="Email of registered user"
                            onChange={(e) => { setEmail(e.target.value); setInviteError(null); }} />
                        <select className="input w-28 shrink-0" value={role}
                            onChange={(e) => setRole(e.target.value as BoardRole)}>
                            <option value="Editor">Editor</option>
                            <option value="Viewer">Viewer</option>
                        </select>
                    </div>
                    <button type="submit" className="btn-primary">Invite</button>
                    <ErrorMessage message={inviteError} />
                </form>
            )}
        </section>
     );
}
