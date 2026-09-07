namespace Kanban.Domain.Entities;

public class Card
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ColumnId { get; set; }
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }

    public Column Column { get; set; } = null!;
}
