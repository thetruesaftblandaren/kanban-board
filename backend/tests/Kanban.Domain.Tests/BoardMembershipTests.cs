using Kanban.Domain.Entities;

namespace Kanban.Domain.Tests;

public class BoardMembershipTests
{
    [Fact]
    public void IsMember_ForOwner_ReturnsTrue()
    {
        var ownerId = Guid.NewGuid();

        var board = Board.Create("Test", ownerId);

        var member = Assert.Single(board.Members);
        Assert.Equal(ownerId, member.UserId);
    }

    [Fact]
    public void IsMember_ForStranger_ReturnsFalse()
    {
        var strangerId = Guid.NewGuid();

        var board = Board.Create("Test", Guid.NewGuid());

        var member = Assert.Single(board.Members);
        Assert.NotEqual(strangerId, member.UserId);
    }

    [Fact(Skip = "Pending: board member invitiations are not implemented yet")]
    public void AddMember_AddsMember()
    {
        
    }

    [Fact(Skip = "Pending: board member invitiations are not implemented yet")]
    public void AddMember_WithOwnerRole_Throws()
    {
        
    }

    [Fact(Skip = "Pending: board member invitiations are not implemented yet")]
    public void AddMember_Duplicate_Throws()
    {
        
    }
}
