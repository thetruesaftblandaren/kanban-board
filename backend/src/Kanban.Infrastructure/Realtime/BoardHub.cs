using Microsoft.AspNetCore.SignalR;

namespace Kanban.Infrastructure.Realtime;

public class BoardHub : Hub
{
    public async Task JoinBoard(string boardId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(boardId));
    }

    public async Task LeaveBoard(string boardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(boardId));
    }

    public static string GroupName(string boardId)
    {
        return $"board-{boardId}";
    }
}
