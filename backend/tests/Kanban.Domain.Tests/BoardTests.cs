using Kanban.Domain.Entities;

namespace Kanban.Domain.Tests;

public class BoardTests
{
    [Fact]
    public void Create_SetsNameOwnerAndCreatedAt()
    {
        var before = DateTime.UtcNow;

        var board = Board.Create("Test", Guid.NewGuid());

        var after = DateTime.UtcNow;

        Assert.Equal("Test", board.Name);
        Assert.InRange(board.CreatedAt, before, after);
    }

    [Fact]
    public void Create_AddsOwnerAsMemberWithOwnerRole()
    {
        var ownerId = Guid.NewGuid();

        var board = Board.Create("Test", ownerId);

        var member = Assert.Single(board.Members);
        Assert.Equal(ownerId, member.UserId);
        Assert.Equal(BoardRole.Owner, member.Role);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyName_Throws(string? name)
    {
        Assert.Throws<ArgumentException>(() => Board.Create(name!, Guid.NewGuid()));
    }

    [Fact]
    public void Rename_ChangesName()
    {
        var board = Board.Create("Test", Guid.NewGuid());

        board.Rename("Test2");

        Assert.Equal("Test2", board.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Rename_WithEmptyName_Throws(string? name)
    {
        var board = Board.Create("Test", Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => board.Rename(name!));
    }
}
