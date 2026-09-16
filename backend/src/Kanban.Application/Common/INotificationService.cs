namespace Kanban.Application.Common;

public interface INotificationService
{
    Task NotifyCardMovedAsync(Guid boardId, Guid cardId, Guid columnId, int newOrder);
    Task NotifyCardCreatedAsync(Guid boardId, Guid cardId, Guid columnId, string title, string? description, int order);
    Task NotifyCardUpdatedAsync(Guid boardId, Guid cardId, string title, string? description);
    Task NotifyCardDeletedAsync(Guid boardId, Guid cardId);
}
