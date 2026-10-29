using Kanban.Application.Boards;
using Kanban.Application.Common;
using Kanban.Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace Kanban.Infrastructure.Realtime;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<BoardHub> _hubContext;
    private readonly ConnectionTracker _tracker;

    public SignalRNotificationService(IHubContext<BoardHub> hubContext, ConnectionTracker tracker)
    {
        _hubContext = hubContext;
        _tracker = tracker;
    }

    /* Boards */

    public async Task NotifyBoardCreatedAsync(Guid boardId, string name)
    {
        await _hubContext.Clients
            .Group(BoardHub.GroupName(boardId.ToString()))
            .SendAsync("BoardCreated", new { boardId, name });
    }

    public async Task NotifyBoardRenamedAsync(Guid boardId, string name)
    {
        await _hubContext.Clients
            .Group(BoardHub.GroupName(boardId.ToString()))
            .SendAsync("BoardRenamed", new { boardId, name });
    }

    public async Task NotifyBoardDeletedAsync(Guid boardId)
    {
        await _hubContext.Clients
            .Group(BoardHub.GroupName(boardId.ToString()))
            .SendAsync("BoardDeleted", new { boardId });
    }

    public async Task NotifyBoardListChangedAsync(IEnumerable<Guid> userIds)
    {
        var groups = userIds.Distinct().Select(BoardHub.UserGroupName).ToList();
        if (groups.Count == 0) return;

        await _hubContext.Clients.Groups(groups).SendAsync("BoardListChanged");
    }

    public async Task NotifyMemberAddedAsync(Guid boardId, Guid userId, string displayName, BoardRole role)
    {
        await _hubContext.Clients
            .Group(BoardHub.GroupName(boardId.ToString()))
            .SendAsync("MemberAdded", new { userId, displayName, role });
    }

    public async Task NotifyMemberRoleChangedAsync(Guid boardId, Guid userId, string displayName, BoardRole role)
    {
        await _hubContext.Clients
            .Group(BoardHub.GroupName(boardId.ToString()))
            .SendAsync("MemberRoleChanged", new { userId, displayName, role });
    }

    public async Task NotifyMemberRemovedAsync(Guid boardId, Guid userId)
    {
        var group = BoardHub.GroupName(boardId.ToString());

        await _hubContext.Clients
            .Group(group)
            .SendAsync("MemberRemoved", new { userId });
        
        foreach (var connectionId in _tracker.GetConnections(userId))
            await _hubContext.Groups.RemoveFromGroupAsync(connectionId, group);
    }

    /* Columns */

    public async Task NotifyColumnMovedAsync(Guid boardId, Guid columnId, int newOrder)
    {
        await _hubContext.Clients
            .Group(BoardHub.GroupName(boardId.ToString()))
            .SendAsync("ColumnMoved", new { columnId, newOrder });
    }

    public async Task NotifyColumnCreatedAsync(Guid boardId, Guid columnId, string name, int order)
    {
        await _hubContext.Clients
            .Group(BoardHub.GroupName(boardId.ToString()))
            .SendAsync("ColumnCreated", new { columnId, name, order });
    }

    public async Task NotifyColumnRenamedAsync(Guid boardId, Guid columnId, string name)
    {
        await _hubContext.Clients
            .Group(BoardHub.GroupName(boardId.ToString()))
            .SendAsync("ColumnRenamed", new { columnId, name });
    }

    public async Task NotifyColumnDeletedAsync(Guid boardId, Guid columnId)
    {
        await _hubContext.Clients
            .Group(BoardHub.GroupName(boardId.ToString()))
            .SendAsync("ColumnDeleted", new { columnId });
    }

    /* Cards */

    public async Task NotifyCardMovedAsync(Guid boardId, Guid cardId, Guid columnId, int newOrder)
    {
        await _hubContext.Clients
            .Group(BoardHub.GroupName(boardId.ToString()))
            .SendAsync("CardMoved", new { cardId, columnId, newOrder });
    }

    public async Task NotifyCardCreatedAsync(Guid boardId, Guid cardId, Guid columnId, string title, string? description, CardCategory category, int order)
    {
        await _hubContext.Clients
            .Group(BoardHub.GroupName(boardId.ToString()))
            .SendAsync("CardCreated", new { cardId, columnId, title, description, category, order });
    }

    public async Task NotifyCardUpdatedAsync(Guid boardId, Guid cardId, string title, string? description, CardCategory category)
    {
        await _hubContext.Clients
            .Group(BoardHub.GroupName(boardId.ToString()))
            .SendAsync("CardUpdated", new { cardId, title, description, category });
    }

    public async Task NotifyCardDeletedAsync(Guid boardId, Guid cardId)
    {
        await _hubContext.Clients
            .Group(BoardHub.GroupName(boardId.ToString()))
            .SendAsync("CardDeleted", new { cardId });
    }

}
