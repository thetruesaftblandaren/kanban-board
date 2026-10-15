using Kanban.Domain.Entities;

namespace Kanban.Application.Cards;

public record CardResponse(Guid Id, string Title, string? Description, CardCategory Category, Guid ColumnId, int Order, DateTime CreatedAt);
