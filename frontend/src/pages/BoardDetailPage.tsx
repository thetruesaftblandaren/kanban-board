import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { getColumns, getCards } from "../api/boards";
import { getConnection } from "../api/signalr";
import type { ColumnResponse, CardResponse } from "../types/api";

interface CardMovedPayload {
  cardId: string;
  columnId: string;
  newOrder: number;
}

export default function BoardDetailPage() {
  const { boardId } = useParams<{ boardId: string }>();
  const [columns, setColumns] = useState<ColumnResponse[]>([]);
  const [cards, setCards] = useState<CardResponse[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

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

    connection.start().then(() => {
      connection.invoke("JoinBoard", boardId);
    });

    return () => {
      connection.invoke("LeaveBoard", boardId);
      connection.off("CardMoved", handleCardMoved);
    };
  }, [boardId]);

  if (loading) return <p>Loading...</p>;
  if (error) return <p style={{ color: "red" }}>{error}</p>;

  return (
    <div style={{ display: "flex", gap: "1rem" }}>
      {columns
        .sort((a, b) => a.order - b.order)
        .map((column) => (
          <div key={column.id} style={{ border: "1px solid #ccc", padding: "1rem", minWidth: "200px" }}>
            <h2>{column.name}</h2>
            <ul>
              {cards
                .filter((card) => card.columnId === column.id)
                .sort((a, b) => a.order - b.order)
                .map((card) => (
                  <li key={card.id} style={{ marginBottom: "0.5rem" }}>
                    <strong>{card.title}</strong>
                    {card.description && <p>{card.description}</p>}
                  </li>
                ))}
            </ul>
          </div>
        ))}
    </div>
  );
}
