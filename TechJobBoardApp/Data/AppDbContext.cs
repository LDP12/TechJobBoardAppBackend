using FreelanceJobBoard.Models;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Application>()
            .Property(a => a.Status)
            .HasConversion<string>();
    }
}