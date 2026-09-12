import { useDraggable } from "@dnd-kit/core";
import type { CardResponse } from "../types/api";

interface Props {
    card: CardResponse;
}

export default function DraggableCard({ card }: Props) {
    const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({
        id: card.id,
    });

    const style = transform
    ? {
        transform: `translate(${transform.x}px, ${transform.y}px)`,
        opacity: isDragging ? 0.5 : 1,
      }
    : undefined;

    return (
        <li
            ref={setNodeRef}
            style={{
                border: "1px solid #ddd",
                padding: "0.5rem",
                marginBottom: "0.5rem",
                backgroundColor: "#fafafa",
                cursor: "grab",
                ...style,
            }}
            {...listeners}
            {...attributes}
        >
            <strong>{card.title}</strong>
            {card.description && <p>{card.description}</p>}
        </li>
    )
}
