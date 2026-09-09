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
        var order = Cards.Count;
        var card = Card.Create(title, description, Id, order);
        _cards.Add(card);
        return card;
    }

    internal Card RemoveCard(Guid cardId)
    {
        var card = _cards.FirstOrDefault(c => c.Id == cardId)
            ?? throw new InvalidOperationException("Card not found in this column.");
        
        _cards.Remove(card);
        Reorder();
        return card;
    }

    internal void InsertCard(Card card, int order)
    {
        card.AssignToColumn(Id);
        _cards.Insert(Math.Clamp(order, 0, _cards.Count), card);
        Reorder();
    }

    private void Reorder()
    {
        for (int i = 0; i < _cards.Count; i++)
        {
            _cards[i].SetOrder(i);
        }
    }
}
