import { useDroppable } from "@dnd-kit/core";
import type { ColumnResponse, CardResponse } from "../types/api";
import DraggableCard from "./DraggableCard";

interface Props {
    column: ColumnResponse;
    cards: CardResponse[];
}

export default function DroppableColumn({ column, cards }: Props) {
    const { setNodeRef, isOver } = useDroppable({ id: column.id });

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
            <h2>{column.name}</h2>
            <ul style={{ listStyle: "none", padding: 0 }}>
                {cards.map((card) => (
                    <DraggableCard key={card.id} card={card} />
                ))}
            </ul>
        </div>
    );
}
