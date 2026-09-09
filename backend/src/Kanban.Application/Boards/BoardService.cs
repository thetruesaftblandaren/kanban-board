using Kanban.Application.Common;
using Kanban.Domain.Entities;

namespace Kanban.Application.Boards;

public class BoardService : IBoardService
{
    private readonly IBoardRepository _boardRepository;

    public BoardService(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Result<BoardResponse>> CreateBoardAsync(Guid userId, CreateBoardRequest request)
    {
        var board = new Board
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _boardRepository.AddAsync(board);

        var ownerMembership = new BoardMember
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BoardId = board.Id,
            Role = BoardRole.Owner
        };

        await _boardRepository.AddMemberAsync(ownerMembership);
        await _boardRepository.SaveChangesAsync();

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
        var isMember = await _boardRepository.IsUserMemberAsync(boardId, userId);
        if (!isMember)
            return Result<BoardResponse>.Fail("You do not have access to this board.");

        var board = await _boardRepository.GetByIdAsync(boardId);
        if (board is null)
            return Result<BoardResponse>.Fail("Board not found.");

        return Result<BoardResponse>.Ok(new BoardResponse(board.Id, board.Name, board.OwnerId, board.CreatedAt));
    }
}
