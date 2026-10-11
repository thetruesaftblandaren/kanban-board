using Kanban.Domain.Entities;

namespace Kanban.Domain.Tests;

public class BoardColumnTests : IDisposable
{
    Board? board;

    public BoardColumnTests()
    {
        board = Board.Create("Test", Guid.NewGuid());
    }

    public void Dispose()
    {
        board = null;
    }

    [Fact]
    public void AddColumn_AssignsSequentialOrder()
    {
        var col0 = board!.AddColumn("Test0");
        var col1 = board.AddColumn("Test1");
        var col2 = board.AddColumn("Test2");

        Assert.True(board.Columns.Count == 3);
        Assert.Equal(0, col0.Order);
        Assert.Equal(1, col1.Order);
        Assert.Equal(2, col2.Order);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void AddColumn_WithEmptyName_Throws(string? name)
    {
        Assert.Throws<ArgumentException>(() => board!.AddColumn(name!));
    }

    [Fact]
    public void RenameColumn_ChangesName()
    {
        var col = board!.AddColumn("Test");

        board.RenameColumn(col.Id, "Test2");

        Assert.Equal("Test2", col.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void RenameColumn_WithEmptyName_Throws(string? name)
    {
        var col = board!.AddColumn("Test");

        Assert.Throws<ArgumentException>(() => board.RenameColumn(col.Id, name!));
    }

    [Fact]
    public void RenameColumn_UnknownColumn_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => board!.RenameColumn(Guid.NewGuid(), "Test"));
    }

    [Fact]
    public void DeleteColumn_RemovesColumn()
    {
        var col = board!.AddColumn("Test");

        board.DeleteColumn(col.Id);

        Assert.DoesNotContain(board.Columns.FirstOrDefault((c) => c.Id == col.Id), board.Columns);
    }

    [Fact]
    public void DeleteColumn_UnknownColumn_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => board!.DeleteColumn(Guid.NewGuid()));
    }
}
