namespace Kanban.Domain.Entities;

public class Card
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ColumnId { get; set; }
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }

    private Card() {}

    internal static Card Create(string title, string? description, Guid columnId, int order)
    {
        return new Card
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            ColumnId = columnId,
            Order = order,
            CreatedAt = DateTime.UtcNow
        };
    }

    internal void SetOrder(int order)
    {
        Order = order;
    }

    internal void AssignToColumn(Guid columnId)
    {
        ColumnId = columnId;
    }
}
