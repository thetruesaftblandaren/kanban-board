using Kanban.Application.Common;

namespace Kanban.Application.Columns;

public interface IColumnService
{
    Task<Result<ColumnResponse>> CreateColumnAsync(Guid userId, Guid boardId, CreateColumnRequest request);
    Task<Result<ICollection<ColumnResponse>>> GetColumnsForBoardAsync(Guid userId, Guid boardId);
    Task<Result<ColumnResponse>> RenameColumnAsync(Guid userId, Guid boardId, Guid columnId, UpdateColumnRequest request);
    Task<Result<bool>> DeleteColumnAsync(Guid userId, Guid boardId, Guid columnId);
}
