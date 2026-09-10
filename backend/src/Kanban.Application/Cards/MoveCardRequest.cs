namespace Kanban.Application.Cards;

public record MoveCardRequest(Guid TargetColumnId, int NewOrder);
