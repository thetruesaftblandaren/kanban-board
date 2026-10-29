import { useCallback, useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  DndContext,
  type DragEndEvent,
  PointerSensor,
  useSensor,
  useSensors,
} from "@dnd-kit/core";
import { SortableContext, horizontalListSortingStrategy } from "@dnd-kit/sortable";
import { getColumns, getCards, moveCard, createColumn, moveColumn, getBoardById, updateBoard, deleteBoard, getMembers } from "../api/boards";
import { ensureConnected, getConnection } from "../api/signalr";
import type { ColumnResponse, CardResponse, BoardResponse, CardCategory, BoardMemberResponse } from "../types/api";
import DroppableColumn from "../components/DroppableColumn";
import ErrorMessage from "../components/ErrorMessage";
import { getCurrentUserId } from "../api/auth";
import MembersPanel from "../components/MembersPanel";

interface BoardRenamedPayload {
  boardId: string;
  name: string;
}

interface ColumnMovedPayload {
  columnId: string;
  newOrder: number;
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
  category: CardCategory;
  order: number;
}

interface CardUpdatedPayload {
  cardId: string;
  title: string;
  description: string | null;
  category: CardCategory;
}

interface CardDeletedPayload {
  cardId: string;
}

async function fetchBoardData(boardId: string) {
  const [board, members, columns] = await Promise.all([
    getBoardById(boardId),
    getMembers(boardId),
    getColumns(boardId),
  ]);

  const cardLists = await Promise.all(columns.map((c) => getCards(boardId, c.id)));

  return { board, members, columns, cards: cardLists.flat() }
}

type BoardData = Awaited<ReturnType<typeof fetchBoardData>>;

// The displayed boardname becomes empty upon trying to set it to an empty string (and failing). Don't know how to fix it since board isn't added in this function.
export default function BoardDetailPage() {
  const [board, setBoard] = useState<BoardResponse | null>(null);
  const { boardId } = useParams<{ boardId: string }>();
  const [boardName, setBoardName] = useState("");
  const [isEditingBoard, setIsEditingBoard] = useState(false);
  const [boardActionError, setBoardActionError] = useState<string | null>(null);

  const navigate = useNavigate();

  const [newColumnName, setNewColumnName] = useState("");
  const [columns, setColumns] = useState<ColumnResponse[]>([]);
  const [createColumnError, setCreateColumnError] = useState<string | null>(null);

  const [cards, setCards] = useState<CardResponse[]>([]);

  const [members, setMembers] = useState<BoardMemberResponse[]>([]);
  const currentUserId = getCurrentUserId();
  const myRole = members.find((m) => m.userId === currentUserId)?.role;
  const isOwner = myRole === "Owner";
  const canEdit = myRole === "Owner" || myRole === "Editor";

  const [loadBoardError, setLoadBoardError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  
  const [dragError, setDragError] = useState<string | null>(null);

  const sensors = useSensors(useSensor(PointerSensor));

  const applyBoardData = useCallback((data: BoardData) => {
    setBoard(data.board);
    setBoardName(data.board.name);
    setMembers(data.members);
    setColumns(data.columns);
    setCards(data.cards);
  }, [])

  useEffect(() => {
    if (!boardId) return;

    fetchBoardData(boardId)
      .then(applyBoardData)
      .catch(() => setLoadBoardError("Failed to load board."))
      .finally(() => setLoading(false));
  }, [boardId, applyBoardData]);

  async function handleUpdateBoardName(e: React.SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!boardId) return;

    try {
      await updateBoard(boardId, boardName)

      setBoard((prev) =>
        prev ? { ...prev, name: boardName } : prev
      );

      setIsEditingBoard(false);
    } catch {
      setBoardActionError("Failed to update board name.");
    }
  }

  async function handleDeleteBoard() {
    if (!boardId) return;

    try {
      await deleteBoard(boardId);
      navigate("/boards");
    } catch {
      setBoardActionError("Failed to delete board. Only the owner can delete the board.");
    }
  }

  async function handleCreateColumn(e: React.SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!boardId) return;

    try {
      await createColumn(boardId, newColumnName);
      setNewColumnName("");
    } catch {
      setCreateColumnError("Failed to create column.");
    }
  }

  useEffect(() => {
    if (!boardId) return;

    let redirectTimer: ReturnType<typeof setTimeout> | undefined;
    let active = true;

    const connection = getConnection();

    function handleBoardRenamed(payload: BoardRenamedPayload) {
      setBoardName(payload.name);
    }

    function redirectToBoardsSoon(message: string) {
      setLoadBoardError(message);
      redirectTimer = setTimeout(() => navigate("/boards"), 2000);
    }

    function handleBoardDeleted() {
      redirectToBoardsSoon("This board has been deleted by its owner.");
    }

    function handleMemberAdded(payload: BoardMemberResponse) {
      setMembers((prev) => {
        if (prev.some((m) => m.userId === payload.userId)) return prev;
        return [...prev, payload];
      })
    }

    function handleMemberRoleChanged(payload: BoardMemberResponse) {
      setMembers((prev) => prev.map((m) => (m.userId === payload.userId ? payload : m)));
    }

    function handleMemberRemoved(payload: { userId: string }) {
      if (payload.userId === getCurrentUserId()) {
        redirectToBoardsSoon("You have been removed fromt his board.");
        return;
      }
      setMembers((prev) => prev.filter((m) => m.userId !== payload.userId));
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

    function handleColumnMoved(payload: ColumnMovedPayload) {
      setColumns((prevColumns) =>
        prevColumns.map((c) =>
          c.id === payload.columnId ? { ...c, order: payload.newOrder } : c
        )
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
            category: payload.category,
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
            ? { ...card, title: payload.title, description: payload.description, category: payload.category }
            : card
        )
      );
    }

    function handleCardDeleted(payload: CardDeletedPayload) {
      setCards((prevCards) => prevCards.filter((card) => card.id !== payload.cardId));
    }

    async function handleReconnected() {
      if (!active) return;

      try {
        await connection.invoke("JoinBoard", boardId);
        const data = await fetchBoardData(boardId!);
        if (!active) return;
        applyBoardData(data);
      } catch {
        if (!active) return;
        redirectToBoardsSoon("Could not restore this board. You may have lost access to it.");
      }
    }

    connection.on("BoardRenamed", handleBoardRenamed);
    connection.on("BoardDeleted", handleBoardDeleted);
    connection.on("MemberAdded", handleMemberAdded);
    connection.on("MemberRoleChanged", handleMemberRoleChanged);
    connection.on("MemberRemoved", handleMemberRemoved);
    connection.on("ColumnMoved", handleColumnMoved);
    connection.on("ColumnCreated", handleColumnCreated);
    connection.on("ColumnRenamed", handleColumnRenamed);
    connection.on("ColumnDeleted", handleColumnDeleted);
    connection.on("CardMoved", handleCardMoved);
    connection.on("CardCreated", handleCardCreated);
    connection.on("CardUpdated", handleCardUpdated);
    connection.on("CardDeleted", handleCardDeleted);

    connection.onreconnected(handleReconnected);

    async function connectAndJoin() {
      await ensureConnected();
      await connection.invoke("JoinBoard", boardId);
    }

    connectAndJoin().catch((err) => console.error("SignalR:", err));

    return () => {
      active = false;

      connection.off("BoardRenamed", handleBoardRenamed);
      connection.off("BoardDeleted", handleBoardDeleted);
      connection.off("MemberAdded", handleMemberAdded);
      connection.off("MemberRoleChanged", handleMemberRoleChanged);
      connection.off("MemberRemoved", handleMemberRemoved);
      connection.off("ColumnMoved", handleColumnMoved);
      connection.off("ColumnCreated", handleColumnCreated);
      connection.off("ColumnRenamed", handleColumnRenamed);
      connection.off("ColumnDeleted", handleColumnDeleted);
      connection.off("CardMoved", handleCardMoved);
      connection.off("CardCreated", handleCardCreated);
      connection.off("CardUpdated", handleCardUpdated);
      connection.off("CardDeleted", handleCardDeleted);

      clearTimeout(redirectTimer);
      if (connection.state === "Connected") {
        connection.invoke("LeaveBoard", boardId).catch(() => {});
      }
    };
  }, [boardId, navigate]);

  useEffect(() => {
    if (!dragError) return;
    const timer = setTimeout(() => setDragError(null), 4000);
    return () => clearTimeout(timer);
  }, [dragError])

  async function handleDragEnd(event: DragEndEvent) {
    const { active, over } = event;
    if (!over || !boardId || !canEdit) return;

    setDragError(null);

    const activeId = active.id as string;
    const overId = over.id as string;
    if (activeId === overId) return;

    if (columns.some((c) => c.id === activeId)) {
      const sortedColumns = [...columns].sort((a, b) => a.order - b.order);

      const overColumnId = columns.some((c) => c.id === overId)
        ? overId
        : cards.find((c) => c.id === overId)?.columnId;

      if (!overColumnId || overColumnId === activeId) return;

      const newOrder = sortedColumns.findIndex((c) => c.id === overColumnId);

      try {
        await moveColumn(boardId, activeId, newOrder);
      } catch {
        setDragError("Failed to move column. Please try again.");
      }
      return;
    }

    const card = cards.find((c) => c.id === activeId);
    if (!card) return;

    const overCard = cards.find((c) => c.id === overId);
    const targetColumnId = overCard ? overCard.columnId : overId;

    const targetColumnCards = cards
      .filter((c) => c.columnId === targetColumnId)
      .sort((a, b) => a.order - b.order);

    let newOrder: number;

    if (card.columnId === targetColumnId) {
      newOrder = overCard
        ? targetColumnCards.findIndex((c) => c.id === overCard.id)
        : targetColumnCards.length - 1;
    } else {
      newOrder = overCard
        ? targetColumnCards.findIndex((c) => c.id === overCard.id)
        : targetColumnCards.length;
    }

    try {
      await moveCard(boardId, activeId, targetColumnId, newOrder);
    } catch {
      setDragError("Failed to move card. Please try again.");
    }
  }

  if (loading) return <p>Loading...</p>;
  if (loadBoardError) return <ErrorMessage message={loadBoardError} />;

  return (
    <DndContext sensors={sensors} onDragEnd={handleDragEnd}>
      <div className="space-y-4">
        <ErrorMessage message={dragError} />
        <ErrorMessage message={boardActionError} />

        {canEdit && (
          isEditingBoard ? (
            <form onSubmit={handleUpdateBoardName} className="flex max-w-md gap-2">
              <input className="input" value={boardName} onChange={(e) => {
                setBoardName(e.target.value);
                setBoardActionError(null);
              }} />
              <button type="submit" className="btn-primary">Save</button>
              <button type="button" className="btn-secondary" onClick={() => {
                setBoardName(board!.name);
                setBoardActionError(null);
                setIsEditingBoard(false);
              }} >
                Cancel
              </button>
            </form>
          ) : (
            <div className="flex items-center gap-2">
              <h1 className="text-2xl font-semibold">{board!.name}</h1>
              <button type="button" className="btn-secondary" onClick={() => {
                setBoardName(board!.name);
                setIsEditingBoard(true);
              }}>Edit</button>
              {isOwner && <button type="button" className="btn-danger" onClick={handleDeleteBoard}>Delete board</button>}
            </div>
          )
        )}

        <MembersPanel
          boardId={boardId!}
          members={members}
          canManage={isOwner}
        />

        <ErrorMessage message={createColumnError} />

        {canEdit && (
          <form onSubmit={handleCreateColumn} className="flex max-w-md gap-2">
            <input
              className="input"
              value={newColumnName}
              onChange={(e) => {
                setNewColumnName(e.target.value);
                setCreateColumnError(null);
              }}
              placeholder="New column name"
            />
            <button type="submit" className="btn-primary shrink-0">Add column</button>
          </form>
        )}

        <SortableContext
          items={[...columns].sort((a, b) => a.order - b.order).map((c) => c.id)}
          strategy={horizontalListSortingStrategy}
        >
          <div className="flex items-start gap-4 overflow-x-auto pb-4">
            {[...columns]
              .sort((a, b) => a.order - b.order)
              .map((column) => (
                <DroppableColumn
                  key={column.id}
                  column={column}
                  cards={cards
                    .filter((c) => c.columnId === column.id)
                    .sort((a, b) => a.order - b.order)}
                  boardId={boardId!}
                  canEdit = {canEdit}
                />
              ))}
          </div>
        </SortableContext>
      </div> 
    </DndContext>
  );
}
