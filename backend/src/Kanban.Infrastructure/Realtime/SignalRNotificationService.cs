using Kanban.Application.Common;
using Microsoft.AspNetCore.SignalR;

namespace Kanban.Infrastructure.Realtime;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<BoardHub> _hubContext;

    public SignalRNotificationService(IHubContext<BoardHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyCardMovedAsync(Guid boardId, Guid cardId, Guid columnId, int newOrder)
    {
        await _hubContext.Clients
            .Group(BoardHub.GroupName(boardId.ToString()))
            .SendAsync("CardMoved", new { cardId, columnId, newOrder });
    }

    public async Task NotifyCardCreatedAsync(Guid boardId, Guid cardId, Guid columnId, string title, string? description, int order)
    {
        await _hubContext.Clients
            .Group(BoardHub.GroupName(boardId.ToString()))
            .SendAsync("CardCreated", new { cardId, columnId, title, description, order });
    }

    public async Task NotifyCardUpdatedAsync(Guid boardId, Guid cardId, string title, string? description)
    {
        await _hubContext.Clients
            .Group(BoardHub.GroupName(boardId.ToString()))
            .SendAsync("CardUpdated", new { cardId, title, description });
    }

    public async Task NotifyCardDeletedAsync(Guid boardId, Guid cardId)
    {
        await _hubContext.Clients
            .Group(BoardHub.GroupName(boardId.ToString()))
            .SendAsync("CardDeleted", new { cardId });
    }

}
