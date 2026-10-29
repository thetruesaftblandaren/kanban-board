using System.Security.Claims;
using Kanban.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Kanban.Infrastructure.Realtime;

[Authorize]
public class BoardHub : Hub
{
    private readonly IBoardRepository _boardRepository;
    private readonly ConnectionTracker _tracker;

    public BoardHub(IBoardRepository boardRepository, ConnectionTracker tracker)
    {
        _boardRepository = boardRepository;
        _tracker = tracker;
    }

    public override async Task OnConnectedAsync()
    {
        if (TryGetUserId(out var userId))
            _tracker.Add(Context.ConnectionId, userId);
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroupName(userId));
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        _tracker.Remove(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task JoinBoard(string boardId)
    {
        if (!TryGetUserId(out var userId) || !Guid.TryParse(boardId, out var id))
            throw new HubException("Invalid request.");
        
        var board = await _boardRepository.GetByIdAsync(id);
        if (board is null || !board.IsMember(userId))
            throw new HubException("You do not have access to this board.");

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

    public static string UserGroupName(Guid userId)
    {
        return $"user-{userId}";
    }

    private bool TryGetUserId(out Guid userId)
    {
        var value = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("sub");
        return Guid.TryParse(value, out userId);
    }
}
