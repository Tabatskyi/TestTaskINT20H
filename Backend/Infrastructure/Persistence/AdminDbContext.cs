using Microsoft.EntityFrameworkCore;
using TestTaskINT20H.Domain.Auth.Entities;

namespace TestTaskINT20H.Infrastructure.Persistence;

public sealed class AdminDbContext(DbContextOptions<AdminDbContext> options) : DbContext(options)
{
    public DbSet<Admin> Admins => Set<Admin>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.ToTable("admins");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasColumnName("id");
            entity.Property(a => a.Username).HasColumnName("username").HasMaxLength(100);
            entity.Property(a => a.PasswordHash).HasColumnName("password_hash");
            entity.HasIndex(a => a.Username).IsUnique();
        });
    }
}
