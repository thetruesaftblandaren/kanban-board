import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { getColumns, getCards } from "../api/boards";
import type { ColumnResponse, CardResponse } from "../types/api";

interface ColumnWithCards extends ColumnResponse {
  cards: CardResponse[];
}

export default function BoardDetailPage() {
  const { boardId } = useParams<{ boardId: string }>();
  const [columns, setColumns] = useState<ColumnWithCards[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!boardId) return;

    async function loadBoard() {
      try {
        const columnList = await getColumns(boardId!);

        const columnsWithCards = await Promise.all(
          columnList.map(async (column) => {
            const cards = await getCards(boardId!, column.id);
            return { ...column, cards };
          })
        );

        setColumns(columnsWithCards);
      } catch {
        setError("Failed to load board.");
      } finally {
        setLoading(false);
      }
    }

    loadBoard();
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
              {column.cards
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
