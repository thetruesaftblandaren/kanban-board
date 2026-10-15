using Kanban.Domain.Entities;

namespace Kanban.Application.Cards;

public record CreateCardRequest(string Title, string? Description, CardCategory Category = CardCategory.Task);
