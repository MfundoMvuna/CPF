using CPF.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CPF.Infrastructure.Data;

public class CpfDbContext : DbContext
{
    public CpfDbContext(DbContextOptions<CpfDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<PanicAlert> PanicAlerts => Set<PanicAlert>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Post> Posts => Set<Post>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(256);
            e.Property(u => u.FullName).HasMaxLength(100);
            e.Property(u => u.PhoneNumber).HasMaxLength(20);
            e.Property(u => u.PasswordHash).HasMaxLength(256);
        });

        modelBuilder.Entity<PanicAlert>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.Status);
            e.HasIndex(a => a.TriggeredAtUtc);
            e.HasOne(a => a.User)
                .WithMany(u => u.PanicAlerts)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Shift>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => new { s.UserId, s.ScheduledStartUtc });
            e.HasOne(s => s.User)
                .WithMany(u => u.Shifts)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.YocoCheckoutId);
            e.HasIndex(p => p.Status);
            e.Property(p => p.Currency).HasMaxLength(3);
            e.Property(p => p.Description).HasMaxLength(200);
            e.HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Post>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.CreatedAtUtc);
            e.Property(p => p.Title).HasMaxLength(200);
            e.Property(p => p.Content).HasMaxLength(5000);
            e.HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
