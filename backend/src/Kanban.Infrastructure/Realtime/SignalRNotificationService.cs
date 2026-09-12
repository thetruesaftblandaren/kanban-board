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
}
