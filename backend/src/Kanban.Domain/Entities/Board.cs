namespace Kanban.Domain.Entities;

public class Board
{
    private readonly List<Column> _columns = new();
    private readonly List<BoardMember> _members = new();

    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }

    public IReadOnlyCollection<Column> Columns => _columns.AsReadOnly();
    public IReadOnlyCollection<BoardMember> Members => _members.AsReadOnly();

    public static Board Create(string name, Guid ownerId)
    {
        var board = new Board
        {
            Id = Guid.NewGuid(),
            Name = name,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow
        };

        board._members.Add(new BoardMember
        {
            Id = Guid.NewGuid(),
            BoardId = board.Id,
            UserId = ownerId,
            Role = BoardRole.Owner
        });

        return board;
    }

    public bool IsMember(Guid userId)
    {
        return _members.Any(m => m.UserId == userId);
    }

    public Column AddColumn(string name)
    {
        var order = _columns.Count;
        var column = Column.Create(name, Id, order);
        _columns.Add(column);
        return column;
    }

    public Card AddCard(Guid columnId, string title, string? description)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId)
            ?? throw new InvalidOperationException("Column not found on this board.");
        
        return column.AddCard(title, description);
    }

    public void MoveCard(Guid cardId, Guid targetColumnId, int newOrder)
    {
        var sourceColumn = _columns.FirstOrDefault(c => c.Cards.Any(card => card.Id == cardId))
            ?? throw new InvalidOperationException("Card not found on this board.");
        
        var targetColumn = _columns.FirstOrDefault(c => c.Id == targetColumnId)
            ?? throw new InvalidOperationException("Target column not found on this board.");
        
        var card = sourceColumn.RemoveCard(cardId);
        targetColumn.InsertCard(card, newOrder);
    }
}
