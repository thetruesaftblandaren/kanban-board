using Kanban.Application.Common;

namespace Kanban.Application.Columns;

public interface IColumnService
{
    Task<Result<ColumnResponse>> CreateColumnAsync(Guid userId, Guid boardId, CreateColumnRequest request);
    Task<Result<ColumnResponse>> GetColumnsForBoardAsync(Guid userId, Guid boardId);
}
