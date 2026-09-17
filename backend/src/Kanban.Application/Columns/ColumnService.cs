using Kanban.Application.Common;

namespace Kanban.Application.Columns;

public class ColumnService : IColumnService
{
    private readonly IBoardRepository _boardRepository;
    private readonly INotificationService _notificationService;

    public ColumnService(IBoardRepository boardRepository, INotificationService notificationService)
    {
        _boardRepository = boardRepository;
        _notificationService = notificationService;
    }

    public async Task<Result<ColumnResponse>> CreateColumnAsync(Guid userId, Guid boardId, CreateColumnRequest request)
    {
        var board = await _boardRepository.GetByIdWithDetailsAsync(boardId);
        if (board is null)
            return Result<ColumnResponse>.Fail("Board not found.");

        if (!board.IsMember(userId))
            return Result<ColumnResponse>.Fail("You do not have access to this board.");

        var column = board.AddColumn(request.Name);
        await _boardRepository.SaveChangesAsync();

        await _notificationService.NotifyColumnCreatedAsync(boardId, column.Id, column.Name, column.Order);

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

    public async Task<Result<ColumnResponse>> RenameColumnAsync(Guid userId, Guid boardId, Guid columnId, UpdateColumnRequest request)
    {
        var board = await _boardRepository.GetByIdWithDetailsAsync(boardId);
        if (board is null)
            return Result<ColumnResponse>.Fail("Board not found.");
        
        if (!board.IsMember(userId))
            return Result<ColumnResponse>.Fail("You do not have access to this board.");

        try
        {
            board.RenameColumn(columnId, request.Name);
            await _boardRepository.SaveChangesAsync();

            var column = board.Columns.First((c) => c.Id == columnId);

            await _notificationService.NotifyColumnRenamedAsync(boardId, column.Id, request.Name);

            return Result<ColumnResponse>.Ok(new ColumnResponse(column.Id, column.Name, column.BoardId, column.Order));
        }
        catch (InvalidOperationException ex)
        {
            return Result<ColumnResponse>.Fail(ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteColumnAsync(Guid userId, Guid boardId, Guid columnId)
    {
        var board= await _boardRepository.GetByIdWithDetailsAsync(boardId);
        if (board is null)
            return Result<bool>.Fail("Board not found.");

        if (!board.IsMember(userId))
            return Result<bool>.Fail("You do not have access to this board.");

        try
        {
            board.DeleteColumn(columnId);
            await _boardRepository.SaveChangesAsync();

            await _notificationService.NotifyColumnDeletedAsync(boardId, columnId);

            return Result<bool>.Ok(true);
        }
        catch (InvalidOperationException ex)
        {
            return Result<bool>.Fail(ex.Message);
        }
    }
}
