namespace Kanban.Domain.Entities;

/// <summary>
/// Aggregate root. Columns, cards and members are only changed through this class,
/// which keeps the invariants in one place: exactly one owner, sequential column/card order,
/// and assignees who are always members.
/// </summary>
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

    /// <summary>
    /// Checks if a user is a member of the board.
    /// </summary>
    /// <param name="userId">The user being checked.</param>
    /// <returns>True if the user is a member of the board. False if not.</returns>
    public bool IsMember(Guid userId)
    {
        return _members.Any(m => m.UserId == userId);
    }

    /// <summary>
    /// Creates a new board.
    /// </summary>
    /// <param name="name">The name of the board.</param>
    /// <param name="ownerId">The owner of the board.</param>
    /// <returns>A new board.</returns>
    /// <exception cref="ArgumentException">The name is empty or whitespace.</exception>
    public static Board Create(string name, Guid ownerId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Board name cannot be empty.", nameof(name));

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

    /// <summary>
    /// Renames the board.
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="ArgumentException">The name is empty or whitespace.</exception>
    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Board name cannot be empty.", nameof(name));

        Name = name;
    }

    /// <summary>
    /// Adds a new column to the end of the board.
    /// </summary>
    /// <param name="name">The name of the column.</param>
    /// <returns>The new column.</returns>
    /// <exception cref="ArgumentException">The name is empty or whitespace.</exception>
    public Column AddColumn(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Column name cannot be empty.", nameof(name));

        var order = _columns.Count;
        var column = Column.Create(name, Id, order);
        _columns.Add(column);
        return column;
    }

    /// <summary>
    /// Renames a column.
    /// </summary>
    /// <param name="columnId">The id of the column.</param>
    /// <param name="name">The new name of the column.</param>
    /// <exception cref="InvalidOperationException">The column cannot be found.</exception>
    public void RenameColumn(Guid columnId, string name)
    {
        var column = _columns.FirstOrDefault((c) => c.Id == columnId)
            ?? throw new InvalidOperationException("Column not found on this board.");

        column.Rename(name);
    }

    /// <summary>
    /// Deletes a column
    /// </summary>
    /// <param name="columnId">The id of the column.</param>
    /// <exception cref="InvalidOperationException">The column cannot be found.</exception>
    public void DeleteColumn(Guid columnId)
    {
        var column = _columns.FirstOrDefault((c) => c.Id == columnId)
            ?? throw new InvalidOperationException("Column not found on this board.");

        _columns.Remove(column);
    }

    /// <summary>
    /// Moves a column to a new <c>Order</c>
    /// </summary>
    /// <param name="columnId">The id of the column.</param>
    /// <param name="newOrder">The new order of the column.</param>
    /// <exception cref="InvalidOperationException">The column cannot be found.</exception>
    public void MoveColumn(Guid columnId, int newOrder)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId)
            ?? throw new InvalidOperationException("Column not found on this board.");

        var sorted = _columns.OrderBy(c => c.Order).ToList();
        sorted.Remove(column);

        var clampedOrder = Math.Clamp(newOrder, 0, sorted.Count);
        sorted.Insert(clampedOrder, column);

        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].Order = i;
        }

        _columns.Clear();
        _columns.AddRange(sorted);
    }

    /// <summary>
    /// Adds a new card to the end of the column.
    /// </summary>
    /// <param name="columnId">The id of the column.</param>
    /// <param name="title">The title of the card.</param>
    /// <param name="description">The description of the card.</param>
    /// <param name="category">The category of the card.</param>
    /// <returns>The new card.</returns>
    /// <exception cref="InvalidOperationException">The column cannot be found.</exception>
    public Card AddCard(Guid columnId, string title, string? description, CardCategory category)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId)
            ?? throw new InvalidOperationException("Column not found on this board.");
        
        return column.AddCard(title, description, category);
    }

    /// <summary>
    /// Updates the title, description, and/or category of a card.
    /// </summary>
    /// <param name="columnId">The id of the column.</param>
    /// <param name="cardId">The id of the card.</param>
    /// <param name="title">The new title of the card.</param>
    /// <param name="description">The new description of the card.</param>
    /// <param name="category">The new category of the card.</param>
    /// <exception cref="InvalidOperationException">The card or column cannot be found.</exception>
    public void UpdateCard(Guid columnId, Guid cardId, string title, string? description, CardCategory category)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId)
            ?? throw new InvalidOperationException("Column not found on this board.");

        var card = column.Cards.FirstOrDefault(c => c.Id == cardId)
            ?? throw new InvalidOperationException("Column not found on this board.");

        card.Update(title, description, category);
    }

    /// <summary>
    /// Deletes a card from the column.
    /// </summary>
    /// <param name="columnId">The id of the column.</param>
    /// <param name="cardId">The id of the card.</param>
    /// <exception cref="InvalidOperationException">The column cannot be found.</exception>
    public void DeleteCard(Guid columnId, Guid cardId)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId)
            ?? throw new InvalidOperationException("Column not found on this board.");

        column.DeleteCard(cardId);
    }

    /// <summary>
    /// Moves a card to another column.
    /// </summary>
    /// <param name="cardId">The id of the card.</param>
    /// <param name="targetColumnId">The id of the column.</param>
    /// <param name="newOrder">The new order of the card in the column.</param>
    /// <exception cref="InvalidOperationException">The source or target column cannot be found.</exception>
    public void MoveCard(Guid cardId, Guid targetColumnId, int newOrder)
    {
        var sourceColumn = _columns.FirstOrDefault(c => c.Cards.Any(card => card.Id == cardId))
            ?? throw new InvalidOperationException("Card not found on this board.");
        
        var targetColumn = _columns.FirstOrDefault(c => c.Id == targetColumnId)
            ?? throw new InvalidOperationException("Target column not found on this board.");
        
        var card = sourceColumn.DeleteCard(cardId);
        targetColumn.InsertCard(card, newOrder);
    }

    /// <summary>
    /// Assigns a member of the board to a card. Replaces the existing assignee if it already has one, and removes the assignee if it's null.
    /// </summary>
    /// <param name="columnId">The id of the column where the card to be assigned is.</param>
    /// <param name="cardId">The id of the card to be assigned to.</param>
    /// <param name="assigneeId">The id of the user to be assigned to the card.</param>
    /// <exception cref="InvalidOperationException">
    ///     <case>The card or column cannot be found.</case>
    ///     <case>The user is not a member of the board.</case>
    /// </exception>
    public void AssignCard(Guid columnId, Guid cardId, Guid? assigneeId)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId)
            ?? throw new InvalidOperationException("Column not found on this board.");
        
        var card = column.Cards.FirstOrDefault(c => c.Id == cardId)
            ?? throw new InvalidOperationException("Card not found in this column.");
        
        if (assigneeId is not null && !IsMember(assigneeId.Value))
            throw new InvalidOperationException("Only board members can be assigned to cards.");

        card.AssignToUser(assigneeId);
    }

    /// <summary>
    /// Gets the role of a member.
    /// </summary>
    /// <param name="userId">The id of the member.</param>
    /// <returns>The role of the member. Null if it's not a member.</returns>
    public BoardRole? GetRole(Guid userId)
    {
        return _members.FirstOrDefault(m => m.UserId == userId)?.Role;
    }

    /// <summary>
    /// Checks if a user is allowed to edit.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>True if the user is allowed to edit the board. False if not.</returns>
    public bool CanEdit(Guid userId)
    {
        return GetRole(userId) is BoardRole.Owner or BoardRole.Editor;
    }

    /// <summary>
    /// Adds a new member to the board.
    /// </summary>
    /// <param name="userId">The id of the user.</param>
    /// <param name="role">The role of the user.</param>
    /// <returns>The new member.</returns>
    /// <exception cref="ArgumentException">
    ///     <case>The role doesn't exist.</case>
    ///     <case>The <c>role</c> argument is <c>BoardRole.Owner</c>. Only one user may be owner, which is assigned at board creation.</case>
    ///     <case>The user is already a member of the board.</case>
    /// </exception>
    public BoardMember AddMember(Guid userId, BoardRole role)
    {
        if (!Enum.IsDefined(role))
            throw new ArgumentException("Invalid role.", nameof(role));
        if (role == BoardRole.Owner)
            throw new ArgumentException("A board can only have one owner, assigned at creation.");
        if (IsMember(userId))
            throw new ArgumentException("This user is already a member of the board.");
        
        var member = new BoardMember { Id = Guid.NewGuid(), BoardId = Id, UserId = userId, Role = role };
        _members.Add(member);
        return member;
    }

    /// <summary>
    /// Removes a member from the board.
    /// </summary>
    /// <param name="userId">The id of the user.</param>
    /// <exception cref="ArgumentException">
    ///     <case>The user is not a member of the board.</case>
    ///     <case>The user is the member of the board which cannot be removed without deleting the board.</case>
    /// </exception>
    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new ArgumentException("User is not a member of this board.");
        
        if (member.Role == BoardRole.Owner)
            throw new ArgumentException("The owner cannot be removed from the board.");
        
        foreach (var card in _columns.SelectMany(c => c.Cards).Where(c => c.AssigneeId == userId))
            card.AssignToUser(null);
        
        _members.Remove(member);
    }

    /// <summary>
    /// Changes the role of a member.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    ///     <case>The role doesn't exist.</case>
    ///     <case>The <c>role</c> argument is <c>BoardRole.Owner</c>. Only one user may be owner which is assigned at creation.</case>
    ///     <case>The user is not a member of the board</case>
    ///     <case>The role of the owner cannot be changed.</case>
    /// </exception>
    public BoardMember ChangeMemberRole(Guid userId, BoardRole role)
    {
        if (!Enum.IsDefined(role))
            throw new ArgumentException("Invalid role.", nameof(role));

        if (role == BoardRole.Owner)
            throw new ArgumentException("Ownership cannot be assigned to another member.");
        
        var member = _members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new ArgumentException("User is not a member of this board.");
        
        if (member.Role == BoardRole.Owner)
            throw new ArgumentException("The owner's role cannot be changed.");
        
        member.Role = role;
        return member;
    }
}
