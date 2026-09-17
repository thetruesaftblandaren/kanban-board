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

    public bool IsMember(Guid userId)
    {
        return _members.Any(m => m.UserId == userId);
    }


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

    public void Rename(string name)
    {
        Name = name;
    }

    public Column AddColumn(string name)
    {
        var order = _columns.Count;
        var column = Column.Create(name, Id, order);
        _columns.Add(column);
        return column;
    }

    public void RenameColumn(Guid columnId, string name)
    {
        var column = _columns.FirstOrDefault((c) => c.Id == columnId)
            ?? throw new InvalidOperationException("Column not found on this board.");

        column.Rename(name);
    }

    public void DeleteColumn(Guid columnId)
    {
        var column = _columns.FirstOrDefault((c) => c.Id == columnId)
            ?? throw new InvalidOperationException("Column not found on this board.");

        _columns.Remove(column);
    }

    public Card AddCard(Guid columnId, string title, string? description)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId)
            ?? throw new InvalidOperationException("Column not found on this board.");
        
        return column.AddCard(title, description);
    }

    public void UpdateCard(Guid columnId, Guid cardId, string title, string? description)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId)
            ?? throw new InvalidOperationException("Column not found on this board.");

        var card = column.Cards.FirstOrDefault(c => c.Id == cardId)
            ?? throw new InvalidOperationException("Column not found on this board.");

        card.Update(title, description);
    }

    public void DeleteCard(Guid columnId, Guid cardId)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId)
            ?? throw new InvalidOperationException("Column not found on this board.");

        column.DeleteCard(cardId);
    }

    public void MoveCard(Guid cardId, Guid targetColumnId, int newOrder)
    {
        var sourceColumn = _columns.FirstOrDefault(c => c.Cards.Any(card => card.Id == cardId))
            ?? throw new InvalidOperationException("Card not found on this board.");
        
        var targetColumn = _columns.FirstOrDefault(c => c.Id == targetColumnId)
            ?? throw new InvalidOperationException("Target column not found on this board.");
        
        var card = sourceColumn.DeleteCard(cardId);
        targetColumn.InsertCard(card, newOrder);
    }
}
