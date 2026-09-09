using Kanban.Application.Common;

namespace Kanban.Application.Columns;

public class ColumnService : IColumnService
{
    private readonly IBoardRepository _boardRepository;

    public ColumnService(IBoardRepository boardRepository) => _boardRepository = boardRepository;

    public async Task<Result<ColumnResponse>> CreateColumnAsync(Guid userId, Guid boardId, CreateColumnRequest request)
    {
        var board = await _boardRepository.GetByIdWithDetailsAsync(boardId);
        if (board is null)
            return Result<ColumnResponse>.Fail("Board not found.");

        if (!board.IsMember(userId))
            return Result<ColumnResponse>.Fail("You do not have access to this board.");

        var column = board.AddColumn(request.Name);
        await _boardRepository.SaveChangesAsync();

        return Result<ColumnResponse>.Ok(new ColumnResponse(column.Id, column.Name, column.BoardId, column.Order));
    }

    public async Task<Result<ICollection<ColumnResponse>>> GetColumnsForBoardAsync(Guid userId, Guid boardId)
    {
        var board = await _boardRepository.GetByIdWithDetailsAsync(boardId);
        if (board is null)
            return Result<ICollection<ColumnResponse>>.Fail("Board not found.");

        if (!board.IsMember(userId))
            return Result<ICollection<ColumnResponse>>.Fail("You do not have access to this board.");

        var response = board.Columns
            .OrderBy(c => c.Order)
            .Select(c => new ColumnResponse(c.Id, c.Name, c.BoardId, c.Order))
            .ToList();

        return Result<ICollection<ColumnResponse>>.Ok((ICollection<ColumnResponse>)response);
    }
}
