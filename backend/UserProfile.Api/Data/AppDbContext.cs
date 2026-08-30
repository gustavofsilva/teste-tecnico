using Microsoft.EntityFrameworkCore;
using UserProfile.Api.Domain;

namespace UserProfile.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        var user = builder.Entity<User>();
        user.HasKey(x => x.Id);
        user.HasIndex(x => x.Email).IsUnique();
        user.Property(x => x.Name).HasMaxLength(120).IsRequired();
        user.Property(x => x.Email).HasMaxLength(254).IsRequired();
        user.Property(x => x.PasswordHash).IsRequired();
    }
}
