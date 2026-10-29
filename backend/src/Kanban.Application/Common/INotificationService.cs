using Kanban.Application.Boards;
using Kanban.Domain.Entities;

namespace Kanban.Application.Common;

public interface INotificationService
{
    /* Boards */

    Task NotifyBoardCreatedAsync(Guid boardId, string name);
    Task NotifyBoardRenamedAsync(Guid boardId, string name);
    Task NotifyBoardDeletedAsync(Guid boardId);
    Task NotifyBoardListChangedAsync(IEnumerable<Guid> userIds);
    Task NotifyMemberAddedAsync(Guid boardId, Guid userId, string displayName, BoardRole role);
    Task NotifyMemberRoleChangedAsync(Guid boardId, Guid userId, string displayName, BoardRole role);
    Task NotifyMemberRemovedAsync(Guid boardId, Guid userId);

    /* Columns */
    
    Task NotifyColumnMovedAsync(Guid boardId, Guid columnId, int newOrder);
    Task NotifyColumnCreatedAsync(Guid boardId, Guid columnId, string name, int order);
    Task NotifyColumnRenamedAsync(Guid boardId, Guid columnId, string name);
    Task NotifyColumnDeletedAsync(Guid boardId, Guid columnId);

    /* Cards */

    Task NotifyCardMovedAsync(Guid boardId, Guid cardId, Guid columnId, int newOrder);
    Task NotifyCardCreatedAsync(Guid boardId, Guid cardId, Guid columnId, string title, string? description, CardCategory category, int order);
    Task NotifyCardUpdatedAsync(Guid boardId, Guid cardId, string title, string? description, CardCategory category);
    Task NotifyCardDeletedAsync(Guid boardId, Guid cardId);
}
