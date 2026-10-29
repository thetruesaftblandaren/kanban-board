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

    [Theory]
    [InlineData(BoardRole.Editor)]
    [InlineData(BoardRole.Viewer)]
    public void AddMember_AddsMember(BoardRole role)
    {
        var newMemberId = Guid.NewGuid();

        var board = Board.Create("Test", Guid.NewGuid());

        board.AddMember(newMemberId, role);

        Assert.True(board.IsMember(newMemberId));
        Assert.NotNull(board.Members.FirstOrDefault(m => m.UserId == newMemberId));
    }

   [Fact]
    public void AddMember_CanEdit()
    {
        var ownerId = Guid.NewGuid();
        var editorId = Guid.NewGuid();

        var board = Board.Create("Test", ownerId);

        board.AddMember(editorId, BoardRole.Editor);

        Assert.Equal(BoardRole.Owner, board.GetRole(ownerId));
        Assert.Equal(BoardRole.Editor, board.GetRole(editorId));

        Assert.True(board.CanEdit(ownerId));
        Assert.True(board.CanEdit(editorId));
    }

    [Fact]
    public void AddMember_WithOwnerRole_Throws()
    {
        var board = Board.Create("Test", Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => board.AddMember(Guid.NewGuid(), BoardRole.Owner));
    }

    [Fact]
    public void AddMember_Duplicate_Throws()
    {
        var newMemberId = Guid.NewGuid();

        var board = Board.Create("Test", Guid.NewGuid());

        board.AddMember(newMemberId, BoardRole.Viewer);

        Assert.Throws<ArgumentException>(() => board.AddMember(newMemberId, BoardRole.Editor));
    }


    [Fact]
    public void AddMember_WithUnknownRole_Throws()
    {
        var board = Board.Create("Test", Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => board.AddMember(Guid.NewGuid(), (BoardRole)99));
    }

    [Fact]
    public void ChangeRole_FromViewerToEditor_CanEdit()
    {
        var newMemberId = Guid.NewGuid();

        var board = Board.Create("Test", Guid.NewGuid());

        board.AddMember(newMemberId, BoardRole.Viewer);

        Assert.False(board.CanEdit(newMemberId));

        board.ChangeMemberRole(newMemberId, BoardRole.Editor);

        Assert.True(board.CanEdit(newMemberId));
    }

    [Fact]
    public void ChangeRole_FromEditorToViewer_CannotEdit()
    {
        var newMemberId = Guid.NewGuid();

        var board = Board.Create("Test", Guid.NewGuid());

        board.AddMember(newMemberId, BoardRole.Editor);

        Assert.True(board.CanEdit(newMemberId));

        board.ChangeMemberRole(newMemberId, BoardRole.Viewer);

        Assert.False(board.CanEdit(newMemberId));
    }

    [Theory]
    [InlineData(BoardRole.Editor)]
    [InlineData(BoardRole.Viewer)]
    public void ChangeRole_FromOwner_Throws(BoardRole role)
    {
        var ownerId = Guid.NewGuid();

        var board = Board.Create("Test", ownerId);

        var member = Assert.Single(board.Members);

        Assert.Equal(BoardRole.Owner, member.Role);

        Assert.Throws<ArgumentException>(() => board.ChangeMemberRole(ownerId, role));
    }

    [Fact]
    public void GetRole_UnknownUser_ReturnsNull()
    {
        var board = Board.Create("Test", Guid.NewGuid());

        Assert.Null(board.GetRole(Guid.NewGuid()));
    }

    [Fact]
    public void RemoveMember_IsMember_RemovesMember()
    {
        var memberId = Guid.NewGuid();

        var board = Board.Create("Test", Guid.NewGuid());

        board.AddMember(memberId, BoardRole.Editor);

        var member = board.Members.FirstOrDefault(m => m.UserId == memberId);

        Assert.True(board.IsMember(member!.UserId));

        board.RemoveMember(member.UserId);

        Assert.False(board.IsMember(member.UserId));
        Assert.Null(board.Members.FirstOrDefault(m => m.UserId == memberId));
    }

    [Fact]
    public void RemoveMember_IsNotMember_Throws()
    {
        var board = Board.Create("Test", Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => board.RemoveMember(Guid.NewGuid()));
    }
}
