using Kanban.Domain.Entities;
using Kanban.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kanban.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<User> DomainUsers => Set<User>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<Column> Columns => Set<Column>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<BoardMember> BoardMembers => Set<BoardMember>();
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Board>(b =>
        {
            b.HasKey(x => x.Id);
            b.Navigation(x => x.Columns).UsePropertyAccessMode(PropertyAccessMode.Field);
            b.Navigation(x => x.Members).UsePropertyAccessMode(PropertyAccessMode.Field);
            b.HasMany(x => x.Columns).WithOne().HasForeignKey(c => c.BoardId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Members).WithOne(m => m.Board).HasForeignKey(m => m.BoardId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Column>(c =>
        {
            c.HasKey(x => x.Id);
            c.Navigation(x => x.Cards).UsePropertyAccessMode(PropertyAccessMode.Field);
            c.HasMany(x => x.Cards).WithOne().HasForeignKey(card => card.ColumnId).OnDelete(DeleteBehavior.Cascade);
        });

        // Should be unique despite having a surrogate key
        builder.Entity<BoardMember>()
            .HasIndex(bm => new { bm.UserId, bm.BoardId })
            .IsUnique();
        
        // Displaying enums as string for clarity and safety
        builder.Entity<BoardMember>()
            .Property(bm => bm.Role)
            .HasConversion<string>();
    }
}
