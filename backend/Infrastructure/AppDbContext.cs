using Microsoft.EntityFrameworkCore;
using TicketManager.API.Domain.Entities;

namespace TicketManager.API.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Priority).HasConversion<string>();
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(1000);
            entity.HasOne(e => e.Ticket)
                  .WithMany(t => t.Comments)
                  .HasForeignKey(e => e.TicketId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
