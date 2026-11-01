using Kanban.Domain.Entities;

namespace Kanban.Domain.Tests;

public class BoardAssignmentTests
{
    Board? board;
    Column? column;
    Card? card;
    Guid memberId;

    public BoardAssignmentTests()
    {
        board = Board.Create("Test", Guid.NewGuid());
        column = board.AddColumn("Test");
        card = board.AddCard(column.Id, "Test", "Test", CardCategory.Task);
        memberId = Guid.NewGuid();
        board.AddMember(memberId, BoardRole.Editor);
    }

    [Fact]
    public void AssignCard_ToMember_SetsAssignee()
    {
        board!.AssignCard(column!.Id, card!.Id, memberId);

        Assert.Equal(memberId, card.AssigneeId);
    }

    [Fact]
    public void AssignCard_ToNull_ClearsMember()
    {
        board!.AssignCard(column!.Id, card!.Id, memberId);

        Assert.NotNull(card.AssigneeId);

        board.AssignCard(column.Id, card.Id, null);

        Assert.Null(card.AssigneeId);
    }

    [Fact]
    public void AssignCard_ToNonMember_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>board!.AssignCard(column!.Id, card!.Id, Guid.NewGuid()));
    }

    [Fact]
    public void AssignCard_UnknownCard_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => board!.AssignCard(column!.Id, Guid.NewGuid(), memberId));
    }

    [Fact]
    public void RemoveMember_ClearsTheirCardAssignments()
    {
        board!.AssignCard(column!.Id, card!.Id, memberId);

        Assert.Equal(memberId, card.AssigneeId);

        board.RemoveMember(memberId);

        Assert.Null(card.AssigneeId);
    }

    [Fact]
    public void MoveCard_KeepsAssignee()
    {
        var column1 = board!.AddColumn("Test");

        board.AssignCard(column!.Id, card!.Id, memberId);

        Assert.Equal(memberId, card.AssigneeId);

        board.MoveCard(card.Id, column1.Id, 1);

        Assert.Equal(memberId, card.AssigneeId);
    }
}
