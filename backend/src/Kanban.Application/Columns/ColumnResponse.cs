namespace Kanban.Application.Columns;

public record ColumnResponse(Guid Id, string Name, Guid BoardId, int Order);
