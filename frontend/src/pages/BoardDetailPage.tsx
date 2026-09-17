import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  DndContext,
  type DragEndEvent,
  PointerSensor,
  useSensor,
  useSensors,
} from "@dnd-kit/core";
import { getColumns, getCards, moveCard, createColumn, getBoardById, updateBoard, deleteBoard } from "../api/boards";
import { getConnection } from "../api/signalr";
import type { ColumnResponse, CardResponse } from "../types/api";
import DroppableColumn from "../components/DroppableColumn";

interface BoardRenamedPayload {
  boardId: string;
  name: string;
}

interface BoardDeletedPayload {
  boardId: string;
}

interface ColumnCreatedPayload {
  columnId: string;
  name: string;
  order: number;
}

interface ColumnRenamedPayload {
  columnId: string;
  name: string;
}

interface ColumnDeletedPayload {
  columnId: string;
}

interface CardMovedPayload {
  cardId: string;
  columnId: string;
  newOrder: number;
}

interface CardCreatedPayload {
  cardId: string;
  columnId: string;
  title: string;
  description: string | null;
  order: number;
}

interface CardUpdatedPayload {
  cardId: string;
  title: string;
  description: string | null;
}

interface CardDeletedPayload {
  cardId: string;
}

export default function BoardDetailPage() {
  const { boardId } = useParams<{ boardId: string }>();
  const [boardName, setBoardName] = useState("");
  const [isEditingBoard, setIsEditingBoard] = useState(false);
  const navigate = useNavigate();
  const [newColumnName, setNewColumnName] = useState("");
  const [columns, setColumns] = useState<ColumnResponse[]>([]);
  const [cards, setCards] = useState<CardResponse[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const sensors = useSensors(useSensor(PointerSensor));

  useEffect(() => {
    if (!boardId) return;

    async function loadBoard() {
      try {
        const board = await getBoardById(boardId!);
        setBoardName(board.name);

        const columnList = await getColumns(boardId!);
        const cardLists = await Promise.all(
          columnList.map((column) => getCards(boardId!, column.id))
        );

        setColumns(columnList);
        setCards(cardLists.flat());
      } catch {
        setError("Failed to load board.");
      } finally {
        setLoading(false);
      }
    }

    loadBoard();
  }, [boardId]);

  async function handleUpdateBoardName(e: React.SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!boardName.trim() || !boardId) return;

    try {
      await updateBoard(boardId, boardName)
      setIsEditingBoard(false);
    } catch {
      setError("Failed to update board name.");
    }
  }

  async function handleDeleteBoard() {
    if (!boardId) return;

    try {
      await deleteBoard(boardId);
      navigate("/boards");
    } catch {
      setError("Failed to delete board. Only the owner can delete the board.");
    }
  }

  async function handleCreateColumn(e: React.SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!newColumnName.trim() || !boardId) return;

    try {
      await createColumn(boardId, newColumnName);
      setNewColumnName("");
    } catch {
      setError("Failed to create column.");
    }
  }

  useEffect(() => {
    if (!boardId) return;

    const connection = getConnection();

    function handleBoardRenamed(payload: BoardRenamedPayload) {
      setBoardName(payload.name);
    }

    function handleBoardDeleted() {
      setError("This board has been deleted by its owner.");
      setTimeout(() => navigate("/boards"), 2000);
    }

    function handleColumnCreated(payload: ColumnCreatedPayload) {
      setColumns((prevColumns) => {
        if (prevColumns.some((c) => c.id === payload.columnId)) return prevColumns;
        return [ ...prevColumns, { id: payload.columnId, name: payload.name, boardId: boardId!, order: payload.order} ];
      })
    }

    function handleColumnRenamed(payload: ColumnRenamedPayload) {
      setColumns((prevColumns) => 
        prevColumns.map((c) =>
          c.id === payload.columnId
            ? { ...c, name: payload.name } : c)
      );
    }

    function handleColumnDeleted(payload: ColumnDeletedPayload) {
      setColumns((prevColumns) =>
        prevColumns.filter((c) => c.id !== payload.columnId))
        setCards((prevCards) => prevCards.filter((c) => c.columnId !== payload.columnId));
    }

    function handleCardMoved(payload: CardMovedPayload) {
      setCards((prevCards) =>
        prevCards.map((card) =>
          card.id === payload.cardId
            ? { ...card, columnId: payload.columnId, order: payload.newOrder }
            : card
        )
      );
    }

    function handleCardCreated(payload: CardCreatedPayload) {
      setCards((prevCards) => {
        if (prevCards.some((c) => c.id === payload.cardId)) return prevCards;
        return [
          ...prevCards,
          {
            id: payload.cardId,
            columnId: payload.columnId,
            title: payload.title,
            description: payload.description,
            order: payload.order,
            createdAt: new Date().toISOString(),
          },
        ];
      });
    }

    function handleCardUpdated(payload: CardUpdatedPayload) {
      setCards((prevCards) =>
        prevCards.map((card) =>
          card.id === payload.cardId
            ? { ...card, title: payload.title, description: payload.description }
            : card
        )
      );
    }

    function handleCardDeleted(payload: CardDeletedPayload) {
      setCards((prevCards) => prevCards.filter((card) => card.id !== payload.cardId));
    }

    connection.on("BoardRenamed", handleBoardRenamed);
    connection.on("BoardDeleted", handleBoardDeleted);
    connection.on("ColumnCreated", handleColumnCreated);
    connection.on("ColumnRenamed", handleColumnRenamed);
    connection.on("ColumnDeleted", handleColumnDeleted);
    connection.on("CardMoved", handleCardMoved);
    connection.on("CardCreated", handleCardCreated);
    connection.on("CardUpdated", handleCardUpdated);
    connection.on("CardDeleted", handleCardDeleted);

    async function connectAndJoin() {
      if (connection.state === "Disconnected") {
        await connection.start();
      }
      if (connection.state === "Connected") {
        await connection.invoke("JoinBoard", boardId);
      }
    }

    connectAndJoin();

    return () => {
      connection.off("BoardRenamed", handleBoardRenamed);
      connection.off("BoardDeleted", handleBoardDeleted);
      connection.off("ColumnCreated", handleColumnCreated);
      connection.off("ColumnRenamed", handleColumnRenamed);
      connection.off("ColumnDeleted", handleColumnDeleted);
      connection.off("CardMoved", handleCardMoved);
      connection.off("CardCreated", handleCardCreated);
      connection.off("CardUpdated", handleCardUpdated);
      connection.off("CardDeleted", handleCardDeleted);
      if (connection.state === "Connected") {
        connection.invoke("LeaveBoard", boardId).catch(() => {});
      }
    };
  }, [boardId]);

  async function handleDragEnd(event: DragEndEvent) {
    const { active, over } = event;
    if (!over || !boardId) return;

    const cardId = active.id as string;
    const targetColumnId = over.id as string;

    const card = cards.find((c) => c.id === cardId);
    if (!card || card.columnId === targetColumnId) return;

    const cardsInTargetColumn = cards.filter((c) => c.columnId === targetColumnId);
    const newOrder = cardsInTargetColumn.length;

    const previousCards = cards;

    setCards((prevCards) =>
      prevCards.map((c) =>
        c.id === cardId ? { ...c, columnId: targetColumnId, order: newOrder } : c
      )
    );

    try {
      await moveCard(boardId, cardId, targetColumnId, newOrder);
    } catch {
      setCards(previousCards);
      setError("Failed to move card. Please try again.");
    }
  }

  if (loading) return <p>Loading...</p>;
  if (error) return <p style={{ color: "red" }}>{error}</p>;

  return (
    <DndContext sensors={sensors} onDragEnd={handleDragEnd}>
      {isEditingBoard ? (
        <form onSubmit={handleUpdateBoardName}>
          <input value={boardName} onChange={(e) => setBoardName(e.target.value)} />
          <button type="submit">Save</button>
          <button type="button" onClick={() => { setBoardName(boardName); setIsEditingBoard(false); }}>Cancel</button>
        </form>
      ) : (
        <div>
          <h1 style={{ display: "inline" }}>{boardName}</h1>
          <button type="button" onClick={() => setIsEditingBoard(true)}>Edit</button>
          <button type="button" onClick={handleDeleteBoard}>Delete board</button>
        </div>
      )}
      <form onSubmit={handleCreateColumn}>
        <input
          value={newColumnName}
          onChange={((e) => setNewColumnName(e.target.value))}
          placeholder="New column name"
        />
        <button type="submit">Add column</button>
      </form>

      <div style={{ display: "flex", gap: "1rem" }}>
        {columns
          .sort((a, b) => a.order - b.order)
          .map((column) => (
            <DroppableColumn
              key={column.id}
              column={column}
              cards={cards.filter((c) => c.columnId === column.id).sort((a, b) => a.order - b.order)}
              boardId={boardId!}
            />
          ))}
      </div>
    </DndContext>
  );
}
