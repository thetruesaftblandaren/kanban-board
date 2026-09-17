namespace Kanban.Application.Common;

public interface INotificationService
{
    /* Boards */

    Task NotifyBoardCreatedAsync(Guid boardId, string name);
    Task NotifyBoardRenamedAsync(Guid boardId, string name);
    Task NotifyBoardDeletedAsync(Guid boardId);

    /* Columns */
    
    Task NotifyColumnCreatedAsync(Guid boardId, Guid columnId, string name, int order);
    Task NotifyColumnRenamedAsync(Guid boardId, Guid columnId, string name);
    Task NotifyColumnDeletedAsync(Guid boardId, Guid columnId);

    /* Cards */

    Task NotifyCardMovedAsync(Guid boardId, Guid cardId, Guid columnId, int newOrder);
    Task NotifyCardCreatedAsync(Guid boardId, Guid cardId, Guid columnId, string title, string? description, int order);
    Task NotifyCardUpdatedAsync(Guid boardId, Guid cardId, string title, string? description);
    Task NotifyCardDeletedAsync(Guid boardId, Guid cardId);
}
