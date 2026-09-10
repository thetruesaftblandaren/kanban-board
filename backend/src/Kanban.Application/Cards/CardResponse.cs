namespace Kanban.Application.Cards;

public record CardResponse(Guid Id, string Title, string? Description, Guid ColumnId, int Order, DateTime CreatedAt);
