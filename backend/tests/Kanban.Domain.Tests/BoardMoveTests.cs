using Kanban.Domain.Entities;

namespace Kanban.Domain.Tests;

public class BoardMoveTests : IDisposable
{
    Board? board;
    Column? column0;
    Column? column1;
    Column? column2;
    Card? card0;
    Card? card1;
    Card? card2;

    public BoardMoveTests()
    {
        board = Board.Create("Test", Guid.NewGuid());
        column0 = board.AddColumn("Test0");
        column1 = board.AddColumn("Test1");
        column2 = board.AddColumn("Test2");
        card0 = board.AddCard(column0.Id, "Test0", "Test0", CardCategory.Task);
        card1 = board.AddCard(column0.Id, "Test1", "Test1", CardCategory.Task);
        card2 = board.AddCard(column0.Id, "Test2", "Test2", CardCategory.Task);
    }

    public void Dispose()
    {
        board = null;
        column0 = null;
        column1 = null;
        column2 = null;
        card0 = null;
        card1 = null;
        card2 = null;
    }

    [Fact]
    public void MoveCard_WithinColumn_ToFront_ReordersWithoutGaps()
    {
        Assert.Equal(0, card0!.Order);
        Assert.Equal(1, card1!.Order);
        Assert.Equal(2, card2!.Order);

        board!.MoveCard(card0.Id, column0!.Id, 4);  // Could also be 2, but testing 4 to ensure that it doesn't leave any gaps
        
        Assert.Equal(2, card0.Order);
        Assert.Equal(0, card1.Order);
        Assert.Equal(1, card2.Order);
    }

    [Fact]
    public void MoveCard_WithinColumn_ToBack()
    {
        Assert.Equal(0, card0!.Order);
        Assert.Equal(1, card1!.Order);
        Assert.Equal(2, card2!.Order);

        board!.MoveCard(card2!.Id, column0!.Id, 0);

        Assert.Equal(0, card2.Order);
        Assert.Equal(1, card0.Order);
        Assert.Equal(2, card1.Order);
    }

    [Fact]
    public void MoveCard_WithinColumn_ToMiddle()
    {
        Assert.Equal(0, card0!.Order);
        Assert.Equal(1, card1!.Order);
        Assert.Equal(2, card2!.Order);

        board!.MoveCard(card2!.Id, column0!.Id, 1);

        Assert.Equal(1, card2.Order);
        Assert.Equal(0, card0.Order);
        Assert.Equal(2, card1.Order);
    }

    [Fact]
    public void MoveCard_ToAnotherColumn_UpdatesBothColumns()
    {
        // Adding some extra cards to check that they're ordered correctly
        var card3 = board!.AddCard(column1!.Id, "Test3", "Test3", CardCategory.Task);
        var card4 = board.AddCard(column1!.Id, "Test4", "Test4", CardCategory.Task);

        Assert.Equal(0, card0!.Order);
        Assert.Equal(1, card1!.Order);
        Assert.Equal(2, card2!.Order);
        Assert.Equal(0, card3.Order);
        Assert.Equal(1, card4.Order);

        Assert.Equal(3, column0!.Cards.Count);
        Assert.Equal(2, column1.Cards.Count);

        board.MoveCard(card1.Id, column1.Id, 0);

        Assert.Equal(0, card0.Order);
        Assert.Equal(1, card2.Order);
        Assert.Equal(0, card1.Order);
        Assert.Equal(1, card3.Order);
        Assert.Equal(2, card4.Order);

        Assert.Equal(2, column0.Cards.Count);
        Assert.Equal(3, column1.Cards.Count);
    }

    [Fact]
    public void MoveCard_ToEmptyColumn()
    {
        Assert.Equal(0, card0!.Order);
        Assert.Equal(1, card1!.Order);
        Assert.Equal(2, card2!.Order);

        Assert.Equal(3, column0!.Cards.Count);
        Assert.Empty(column1!.Cards);

        board!.MoveCard(card1!.Id, column1.Id, 5);  // Using 5 to check that the gaps are removed

        Assert.Equal(0, card1.Order);
        Assert.Equal(0, card0.Order);
        Assert.Equal(1, card2.Order);

        Assert.Equal(2, column0.Cards.Count);
        Assert.Single(column1.Cards);
    }

    [Fact]
    public void MoveCard_WithNegativeOrder_PlacesFirst()
    {
        Assert.Equal(0, card0!.Order);
        Assert.Equal(1, card1!.Order);
        Assert.Equal(2, card2!.Order);

        board!.MoveCard(card2.Id, column0!.Id, -2);

        Assert.Equal(0, card2.Order);
        Assert.Equal(1, card0.Order);
        Assert.Equal(2, card1.Order);
    }

    [Fact]
    public void MoveCard_SameColumnTwice_StaysConsistent()
    {
        Assert.Equal(0, card0!.Order);
        Assert.Equal(1, card1!.Order);
        Assert.Equal(2, card2!.Order);

        board!.MoveCard(card2.Id, column0!.Id, 0);
        
        Assert.Equal(0, card2.Order);
        Assert.Equal(1, card0.Order);
        Assert.Equal(2, card1.Order);

        board.MoveCard(card1.Id, column0.Id, 0);

        Assert.Equal(0, card1.Order);
        Assert.Equal(1, card2.Order);
        Assert.Equal(2, card0.Order);
    }

    [Fact]
    public void MoveCard_KeepsCategory()
    {
        Assert.Equal(CardCategory.Task, card0!.Category);

        board!.MoveCard(card0.Id, column1!.Id, 0);

        Assert.Equal(CardCategory.Task, card0.Category);
    }

    [Fact]
    public void MoveCard_UnknownCard_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => board!.MoveCard(Guid.NewGuid(), column0!.Id, 3));
    }

    [Fact]
    public void MoveColumn_ToFront_ReordersWithoutGaps()
    {
        Assert.Equal(0, column0!.Order);
        Assert.Equal(1, column1!.Order);
        Assert.Equal(2, column2!.Order);

        board!.MoveColumn(column0.Id, 4);

        Assert.Equal(2, column0.Order);
        Assert.Equal(0, column1.Order);
        Assert.Equal(1, column2.Order);
    }

    [Fact]
    public void MoveColumn_ToBack()
    {
        Assert.Equal(0, column0!.Order);
        Assert.Equal(1, column1!.Order);
        Assert.Equal(2, column2!.Order);

        board!.MoveColumn(column2.Id, 0);

        Assert.Equal(0, column2.Order);
        Assert.Equal(1, column0.Order);
        Assert.Equal(2, column1.Order);
    }

    [Fact]
    public void MoveColumn_ToMiddle()
    {
        Assert.Equal(0, column0!.Order);
        Assert.Equal(1, column1!.Order);
        Assert.Equal(2, column2!.Order);

        board!.MoveColumn(column2.Id, 1);

        Assert.Equal(1, column2.Order);
        Assert.Equal(0, column0.Order);
        Assert.Equal(2, column1.Order);
    }

    [Fact]
    public void MoveColumn_WithNegativeOrder_PlacesFirst()
    {
        Assert.Equal(0, column0!.Order);
        Assert.Equal(1, column1!.Order);
        Assert.Equal(2, column2!.Order);

        board!.MoveColumn(column2.Id, -2);

        Assert.Equal(0, column2.Order);
        Assert.Equal(1, column0.Order);
        Assert.Equal(2, column1.Order);
    }

    [Fact]
    public void MoveColumn_UnknownColumn_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => board!.MoveColumn(Guid.NewGuid(), 3));
    }
}
