import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { createBoard, getBoards } from "../api/boards";
import type { BoardResponse } from "../types/api";

export default function BoardsPage() {
  const [boards, setBoards] = useState<BoardResponse[]>([]);
  const [newBoardName, setNewBoardName] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  
  useEffect(() => {
    getBoards()
      .then(setBoards)
      .catch(() => setError("Failed to load boards."))
      .finally(() => setLoading(false));
  }, []);

  async function handleCreateBoard(e: React.SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!newBoardName.trim()) return;

    try {
      const board = await createBoard(newBoardName);
      setBoards((prev) => [...prev, board]);
      setNewBoardName("");
    } catch {
      setError("Failed to create board.");
    }
  };

  if (loading) return <p>Loading...</p>;
  if (error) return <p style={{ color: "red" }}>{error}</p>

  return (
    <div>
      <h1>My Boards</h1>

      <form onSubmit={handleCreateBoard}>
        <input
          value={newBoardName}
          onChange={(e) => setNewBoardName(e.target.value)}
          placeholder="New board name"
        />
        <button type="submit">Create board</button>
      </form>

      {error && <p style={{ color: "red" }}>{error}</p>}
      {boards.length === 0 && <p>No boards yet.</p>}

      <ul>
        {boards.map((board) => (
          <li key={board.id}>
            <button onClick={() => navigate(`/boards/${board.id}`)}>
              {board.name}
            </button>
          </li>
        ))}
      </ul>
    </div>
  );
}
