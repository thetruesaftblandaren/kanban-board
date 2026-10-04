namespace Kanban.Domain.Entities;

public class Column
{
    private readonly List<Card> _cards = new();

    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid BoardId { get; set; }
    public int Order { get; set; }

    public IReadOnlyCollection<Card> Cards => _cards.AsReadOnly();

    public Column() { }

    internal static Column Create(string name, Guid boardId, int order)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Column name cannot be empty.", nameof(name));

        return new Column
        {
            Id = Guid.NewGuid(),
            Name = name,
            BoardId = boardId,
            Order = order
        };
    }

    internal Card AddCard(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Card name cannot be empty.", nameof(title));

        var order = Cards.Count;
        var card = Card.Create(title, description, Id, order);
        _cards.Add(card);
        return card;
    }

    internal void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Column name cannot be empty.", nameof(name));

        Name = name;
    }

    internal Card DeleteCard(Guid cardId)
    {
        var card = _cards.FirstOrDefault(c => c.Id == cardId)
            ?? throw new InvalidOperationException("Card not found in this column.");

        _cards.Remove(card);
        ReorderBasedOnCurrentOrder();
        return card;
    }

    internal void InsertCard(Card card, int order)
    {
        card.AssignToColumn(Id);

        var sorted = _cards.OrderBy(c => c.Order).ToList();
        sorted.Insert(Math.Clamp(order, 0, sorted.Count), card);

        _cards.Clear();
        _cards.AddRange(sorted);

        Reorder();
    }

    private void ReorderBasedOnCurrentOrder()
    {
        var sorted = _cards.OrderBy(c => c.Order).ToList();
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].SetOrder(i);
        }
        _cards.Clear();
        _cards.AddRange(sorted);
    }

    private void Reorder()
    {
        for (int i = 0; i < _cards.Count; i++)
        {
            _cards[i].SetOrder(i);
        }
    }
}
