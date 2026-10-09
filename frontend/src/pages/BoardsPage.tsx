import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { createBoard, getBoards } from "../api/boards";
import type { BoardResponse } from "../types/api";
import BoardListItem from "../components/BoardListItem";
import ErrorMessage from "../components/ErrorMessage";

export default function BoardsPage() {
  const [boards, setBoards] = useState<BoardResponse[]>([]);
  const [newBoardName, setNewBoardName] = useState("");
  const [loadError, setLoadError] = useState<string | null>(null);
  const [createBoardError, setCreateBoardError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  
  useEffect(() => {
    getBoards()
      .then(setBoards)
      .catch(() => setLoadError("Failed to load boards."))
      .finally(() => setLoading(false));
  }, []);

  async function handleCreateBoard(e: React.SubmitEvent<HTMLFormElement>) {
    e.preventDefault();

    try {
      const board = await createBoard(newBoardName);
      setBoards((prev) => [...prev, board]);
      setNewBoardName("");
      setCreateBoardError(null);
    } catch {
      setCreateBoardError("Failed to create board.");
    }
  };

  if (loading) return <p>Loading...</p>;
  if (loadError) return <ErrorMessage message={loadError} />

  return (
    <div>
      <h1>My Boards</h1>

      <ErrorMessage message={createBoardError} />
      <form onSubmit={handleCreateBoard}>
        <input
          value={newBoardName}
          onChange={(e) => {
            setNewBoardName(e.target.value)
            setCreateBoardError(null);
          }}
          placeholder="New board name"
        />
        <button type="submit">Create board</button>
      </form>

      <ErrorMessage message={loadError} />
      {boards.length === 0 && <p>No boards yet.</p>}

      <ul>
        {boards.map((board) => (
          <BoardListItem
            key={board.id}
            board={board}
            onUpdated={(updated) =>
              setBoards((prev) => prev.map((b) => (b.id === updated.id ? updated: b)))
            }
            onDeleted={(boardId) =>
              setBoards((prev) => prev.filter((b) => b.id !== boardId))
            }
          />
          ))}
      </ul>
    </div>
  );
}
