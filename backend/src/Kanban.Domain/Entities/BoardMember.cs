namespace Kanban.Domain.Entities;

public class BoardMember
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BoardId { get; set; }
    public BoardRole Role { get; set; }

    public User User { get; set; } = null!;
    public Board Board { get; set; } = null!;
}
