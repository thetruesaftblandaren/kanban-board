using Kanban.Application.Common;
using Kanban.Domain.Entities;

namespace Kanban.Application.Boards;

public class BoardService : IBoardService
{
    private readonly IBoardRepository _boardRepository;
    private readonly INotificationService _notificationService;

    public BoardService(IBoardRepository boardRepository, INotificationService notificationService)
    {
        _boardRepository = boardRepository;
        _notificationService = notificationService;
    }

    public async Task<Result<BoardResponse>> CreateBoardAsync(Guid userId, CreateBoardRequest request)
    {
        var board = Board.Create(request.Name, userId);

        await _boardRepository.AddAsync(board);
        await _boardRepository.SaveChangesAsync();

        await _notificationService.NotifyBoardCreatedAsync(board.Id, board.Name);

        return Result<BoardResponse>.Ok(new BoardResponse(board.Id, board.Name, board.OwnerId, board.CreatedAt));
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

    public async Task<Result<BoardResponse>> RenameBoardAsync(Guid userId, Guid boardId, UpdateBoardRequest request)
    {
        var board = await _boardRepository.GetByIdWithDetailsAsync(boardId);
        if (board is null)
            return Result<BoardResponse>.Fail("Board not found.");

        if (!board.IsMember(userId))
            return Result<BoardResponse>.Fail("You do not have access to this board.");

        board.Rename(request.Name);
        await _boardRepository.SaveChangesAsync();

        await _notificationService.NotifyBoardRenamedAsync(board.Id, board.Name);

        return Result<BoardResponse>.Ok(new BoardResponse(board.Id, board.Name, board.OwnerId, board.CreatedAt));
    }

    public async Task<Result<bool>> DeleteBoardAsync(Guid userId, Guid boardId)
    {
        var board = await _boardRepository.GetByIdWithDetailsAsync(boardId);
        if (board is null)
            return Result<bool>.Fail("Board not found.");

        if (board.OwnerId != userId)
            return Result<bool>.Fail("Only the board owner can delete this board.");

        await _boardRepository.DeleteAsync(board);
        await _boardRepository.SaveChangesAsync();

        await _notificationService.NotifyBoardDeletedAsync(boardId);

        return Result<bool>.Ok(true);
    }
}
