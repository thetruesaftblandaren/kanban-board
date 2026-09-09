namespace Kanban.Application.Boards;

public record BoardResponse(Guid Id, string Name, Guid OwnerId, DateTime CreatedAt);
