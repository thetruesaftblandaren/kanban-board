namespace Kanban.Application.Common;

public interface INotificationService
{
    Task NotifyCardMovedAsync(Guid boardId, Guid cardId, Guid columnId, int newOrder);
}
