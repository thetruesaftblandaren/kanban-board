using Kanban.Domain.Entities;

namespace Kanban.Domain.Tests;

public class BoardCardTests : IDisposable
{
    Board? board;
    Column? column;

    public BoardCardTests()
    {
        board = Board.Create("Test", Guid.NewGuid());
        column = board.AddColumn("Test");
    }

    public void Dispose()
    {
        board = null;
        column = null;
    }

    [Fact]
    public void AddCard_AssignsSequentialOrderWithinColumn()
    {
        var card0 = board!.AddCard(column!.Id, "Test0", "Test0");
        var card1 = board!.AddCard(column!.Id, "Test1", "Test1");
        var card2 = board!.AddCard(column!.Id, "Test2", "Test2");

        Assert.Equal(0, card0.Order);
        Assert.Equal(1, card1.Order);
        Assert.Equal(2, card2.Order);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void AddCard_WithEmptyDescription_StoresNull(string? desc)
    {
        board!.AddCard(column!.Id, "Test", desc);

        var card = Assert.Single(column.Cards);
        Assert.Null(card.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void AddCard_WithEmptyTitle_Throws(string? title)
    {
        Assert.Throws<ArgumentException>(() => board!.AddCard(column!.Id, title!, "Test"));
    }

    [Fact]
    public void AddCard_ToUnknownColumn_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => board!.AddCard(Guid.NewGuid(), "Test", "Test"));
    }

    [Fact]
    public void UpdateCard_ChangesTitleAndDescription()
    {
        var card = board!.AddCard(column!.Id, "Test", "Test");

        board.UpdateCard(column.Id, card.Id, "Test2", "Test2");

        Assert.Equal("Test2", card.Title);
        Assert.Equal("Test2", card.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void UpdateCard_WithEmptyDescription_StoresNull(string? desc)
    {
        var card = board!.AddCard(column!.Id, "Test", "Test");

        board.UpdateCard(column.Id, card.Id, "Test", desc);

        Assert.Null(card.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void UpdateCard_WithEmptyTitle_Throws(string? title)
    {
        var card = board!.AddCard(column!.Id, "Test", "Test");

        Assert.Throws<ArgumentException>(() => board!.UpdateCard(column.Id, card.Id, title!, "Test"));
    }

    [Fact]
    public void DeleteCard_RemovesCardAndReordersRemaining()
    {
        var card0 = board!.AddCard(column!.Id, "Test0", "Test0");
        var card1 = board.AddCard(column!.Id, "Test1", "Test1");
        var card2 = board.AddCard(column!.Id, "Test2", "Test2");

        Assert.Equal(0, card0.Order);
        Assert.Equal(1, card1.Order);
        Assert.Equal(2, card2.Order);

        board.DeleteCard(column.Id, card1.Id);

        Assert.Equal(0, card0.Order);
        Assert.Equal(1, card2.Order);
    }

    [Fact]
    public void DeleteCard_UnknownColumn_Throws()
    {
        var card = board!.AddCard(column!.Id, "Test", "Test");

        Assert.Throws<InvalidOperationException>(() => board.DeleteCard(Guid.NewGuid(), card.Id));
    }

    [Fact]
    public void DeleteCard_UnknownCard_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => board!.DeleteCard(column!.Id, Guid.NewGuid()));
    }
}
