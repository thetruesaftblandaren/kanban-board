using Microsoft.AspNetCore.Identity;

namespace Kanban.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
}
