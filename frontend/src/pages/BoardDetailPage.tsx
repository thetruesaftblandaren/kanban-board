import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import {
  DndContext,
  type DragEndEvent,
  PointerSensor,
  useSensor,
  useSensors,
} from "@dnd-kit/core";
import { getColumns, getCards, moveCard, createColumn, createCard } from "../api/boards";
import { getConnection } from "../api/signalr";
import type { ColumnResponse, CardResponse } from "../types/api";
import DroppableColumn from "../components/DroppableColumn";

interface CardMovedPayload {
  cardId: string;
  columnId: string;
  newOrder: number;
}

export default function BoardDetailPage() {
  const { boardId } = useParams<{ boardId: string }>();
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

  async function handleCreateColumn(e: React.SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!newColumnName.trim() || !boardId) return;

    try {
      const column = await createColumn(boardId, newColumnName);
      setColumns((prev) => [...prev, column]);
      setNewColumnName("");
    } catch {
      setError("Failed to create column.");
    }
  }

  useEffect(() => {
    if (!boardId) return;

    const connection = getConnection();

    function handleCardMoved(payload: CardMovedPayload) {
      setCards((prevCards) =>
        prevCards.map((card) =>
          card.id === payload.cardId
            ? { ...card, columnId: payload.columnId, order: payload.newOrder }
            : card
        )
      );
    }

    connection.on("CardMoved", handleCardMoved);

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
      connection.off("CardMoved", handleCardMoved);
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

    // Spara det gamla state:t så vi kan återställa vid fel
    const previousCards = cards;

    // Optimistic update: uppdatera UI direkt, innan servern svarar
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
              onCardCreated={(card) => setCards((prev) => [...prev, card])}
            />
          ))}
      </div>
    </DndContext>
  );
}
