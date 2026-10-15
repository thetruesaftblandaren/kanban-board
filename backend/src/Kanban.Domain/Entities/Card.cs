namespace Kanban.Domain.Entities;

public class Card
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CardCategory Category { get; set; }
    public Guid ColumnId { get; set; }
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }

    private Card() {}

    internal static Card Create(string title, string? description, CardCategory category, Guid columnId, int order)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Card title cannot be empty.", nameof(title));

        if (!Enum.IsDefined(category))
            throw new ArgumentException("Invalid card category.", nameof(category));

        return new Card
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = NormalizeDescription(description),
            Category = category,
            ColumnId = columnId,
            Order = order,
            CreatedAt = DateTime.UtcNow
        };
    }

    internal void Update(string title, string? description, CardCategory category)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Card name cannot be empty.", nameof(title));

        if (!Enum.IsDefined(category))
            throw new ArgumentException("Invalid card category.", nameof(category));

        Title = title;
        Description = NormalizeDescription(description);
        Category = category;
    }

    internal void SetOrder(int order)
    {
        Order = order;
    }

    internal void AssignToColumn(Guid columnId)
    {
        ColumnId = columnId;
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description) ? null : description;
    }
}
