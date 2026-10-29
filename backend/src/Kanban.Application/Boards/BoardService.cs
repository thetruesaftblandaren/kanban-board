using Kanban.Application.Auth;
using Kanban.Application.Common;
using Kanban.Domain.Entities;

namespace Kanban.Application.Boards;

public class BoardService : IBoardService
{
    private readonly IBoardRepository _boardRepository;
    private readonly IUserRepository _userRepository;
    private readonly IIdentityService _identityService;
    private readonly INotificationService _notificationService;
    
    public BoardService(IBoardRepository boardRepository, IUserRepository userRepository, IIdentityService identityService, INotificationService notificationService)
    {
        _boardRepository = boardRepository;
        _userRepository = userRepository;
        _identityService = identityService;
        _notificationService = notificationService;
    }

    public async Task<Result<BoardResponse>> CreateBoardAsync(Guid userId, CreateBoardRequest request)
    {
        try
        {
            var board = Board.Create(request.Name, userId);

            await _boardRepository.AddAsync(board);
            await _boardRepository.SaveChangesAsync();

            await _notificationService.NotifyBoardCreatedAsync(board.Id, board.Name);

            return Result<BoardResponse>.Ok(new BoardResponse(board.Id, board.Name, board.OwnerId, board.CreatedAt));
        }
        catch (ArgumentException ex)
        {
            return Result<BoardResponse>.Fail(ex.Message);
        }
    }

    public async Task<ICollection<BoardResponse>> GetBoardsForUserAsync(Guid userId)
    {
        var boards = await _boardRepository.GetForUserAsync(userId);

        return boards
            .Select(b => new BoardResponse(b.Id, b.Name, b.OwnerId, b.CreatedAt))
            .ToList();
    }

    public async Task<Result<BoardResponse>> GetBoardByIdAsync(Guid userId, Guid boardId)
    {
        var board = await _boardRepository.GetByIdAsync(boardId);
        if (board is null)
            return Result<BoardResponse>.Fail("Board not found.");
        
        if (!board.IsMember(userId))
        {
            return Result<BoardResponse>.Fail("You do not have access to this board.");
        }

        return Result<BoardResponse>.Ok(new BoardResponse(board.Id, board.Name, board.OwnerId, board.CreatedAt));
    }

    public async Task<Result<ICollection<BoardMemberResponse>>> GetMembersAsync(Guid userId, Guid boardId)
    {
        var board = await _boardRepository.GetByIdAsync(boardId);
        if (board is null)
            return Result<ICollection<BoardMemberResponse>>.Fail("Board not found.");
        if (!board.IsMember(userId))
            return Result<ICollection<BoardMemberResponse>>.Fail("You do not have access to this board.");
        
        var user = await _userRepository.GetByIdsAsync(board.Members.Select(m => m.UserId));
        var names = user.ToDictionary(u => u.Id, u => u.DisplayName);

        var response = board.Members
            .OrderBy(m => m.Role)
            .Select(m => new BoardMemberResponse(m.UserId, names.GetValueOrDefault(m.UserId, "Unknown"), m.Role))
            .ToList();

        return Result<ICollection<BoardMemberResponse>>.Ok(response);
    }

    public async Task<Result<BoardMemberResponse>> InviteMemberAsync(Guid userId, Guid boardId, InviteMemberRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result<BoardMemberResponse>.Fail("Email cannot be empty.");

        var board = await _boardRepository.GetByIdAsync(boardId);
        if (board is null)
            return Result<BoardMemberResponse>.Fail("Board not found.");
        if (board.GetRole(userId) != BoardRole.Owner)
            return Result<BoardMemberResponse>.Fail("Only the board owner can invite members.");
        
        var inviteeId = await _identityService.FindUserIdByEmailAsync(request.Email);
        if (inviteeId is null)
            return Result<BoardMemberResponse>.Fail("No user with that email address.");

        try
        {
            board.AddMember(inviteeId.Value, request.Role);
            await _boardRepository.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return Result<BoardMemberResponse>.Fail(ex.Message);
        }

        var invitee = await _userRepository.GetByIdAsync(inviteeId.Value);
        var displayName = invitee?.DisplayName ?? request.Email;

        await _notificationService.NotifyMemberAddedAsync(boardId, inviteeId.Value, displayName, request.Role);
        await _notificationService.NotifyBoardListChangedAsync([inviteeId.Value]);

        return Result<BoardMemberResponse>.Ok(
            new BoardMemberResponse(inviteeId.Value, invitee?.DisplayName ?? request.Email, request.Role));
    }

    public async Task<Result<BoardMemberResponse>> ChangeMemberRoleAsync(Guid userId, Guid boardId, Guid memberUserId, ChangeMemberRoleRequest request)
    {
        var board = await _boardRepository.GetByIdAsync(boardId);
        if (board is null)
            return Result<BoardMemberResponse>.Fail("BoardNotFound");
        if (board.GetRole(userId) != BoardRole.Owner)
            return Result<BoardMemberResponse>.Fail("Only the board owner can change roles.");

        try
        {
            board.ChangeMemberRole(memberUserId, request.Role);
            await _boardRepository.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return Result<BoardMemberResponse>.Fail(ex.Message);
        }

        var user = await _userRepository.GetByIdAsync(memberUserId);
        var displayName = user?.DisplayName ?? "Unknown";

        await _notificationService.NotifyMemberRoleChangedAsync(boardId, memberUserId, displayName, request.Role);

        return Result<BoardMemberResponse>.Ok(new BoardMemberResponse(memberUserId, displayName, request.Role));
    }

    public async Task<Result<bool>> RemoveMemberAsync(Guid userId, Guid boardId, Guid memberUserId)
    {
        var board = await _boardRepository.GetByIdAsync(boardId);
        if (board is null)
            return Result<bool>.Fail("BoardNotFound");
        if (board.GetRole(userId) != BoardRole.Owner)
            return Result<bool>.Fail("Only the board owner can change roles.");

        try
        {
            board.RemoveMember(memberUserId);
            await _boardRepository.SaveChangesAsync();
        }
        catch (InvalidOperationException ex)
        {
            return Result<bool>.Fail(ex.Message);
        }

        await _notificationService.NotifyMemberRemovedAsync(boardId, memberUserId);
        await _notificationService.NotifyBoardListChangedAsync([memberUserId]);

        return Result<bool>.Ok(true);
    }

    public async Task<Result<BoardResponse>> RenameBoardAsync(Guid userId, Guid boardId, UpdateBoardRequest request)
    {
        var board = await _boardRepository.GetByIdWithDetailsAsync(boardId);
        if (board is null)
            return Result<BoardResponse>.Fail("Board not found.");

        if (!board.CanEdit(userId))
            return Result<BoardResponse>.Fail("You do not have permission to edit this board.");

        try
        {
            board.Rename(request.Name);
            await _boardRepository.SaveChangesAsync();

            await _notificationService.NotifyBoardRenamedAsync(board.Id, board.Name);
            await _notificationService.NotifyBoardListChangedAsync(board.Members.Select(m => m.UserId));

            return Result<BoardResponse>.Ok(new BoardResponse(board.Id, board.Name, board.OwnerId, board.CreatedAt)); 
        }
        catch (ArgumentException ex)
        {
            return Result<BoardResponse>.Fail(ex.Message);
        }

    }

    public async Task<Result<bool>> DeleteBoardAsync(Guid userId, Guid boardId)
    {
        var board = await _boardRepository.GetByIdWithDetailsAsync(boardId);
        if (board is null)
            return Result<bool>.Fail("Board not found.");

        if (board.OwnerId != userId)
            return Result<bool>.Fail("Only the board owner can delete this board.");

        var memberIds = board.Members.Select(m => m.UserId);

        await _boardRepository.DeleteAsync(board);
        await _boardRepository.SaveChangesAsync();

        await _notificationService.NotifyBoardDeletedAsync(boardId);
        await _notificationService.NotifyBoardListChangedAsync(memberIds);

        return Result<bool>.Ok(true);
    }
}
