using AuthService.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence;

public class WriteDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public WriteDbContext(DbContextOptions<WriteDbContext> options) : base(options) { }
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<AppUser>(e =>
        {
            e.Property(x => x.FirstName).HasMaxLength(100);
            e.Property(x => x.LastName).HasMaxLength(100);
            e.HasMany(x => x.Addresses).WithOne(x => x.User).HasForeignKey(x => x.UserId);
            e.HasMany(x => x.RefreshTokens).WithOne(x => x.User).HasForeignKey(x => x.UserId);
        });

        b.Entity<Address>(e =>
        {
            e.Property(x => x.Line1).IsRequired().HasMaxLength(256);
            e.Property(x => x.City).IsRequired().HasMaxLength(128);
            e.Property(x => x.State).IsRequired().HasMaxLength(128);
            e.Property(x => x.Country).IsRequired().HasMaxLength(128);
            e.Property(x => x.PostalCode).IsRequired().HasMaxLength(32);
        });

        b.Entity<RefreshToken>(e =>
        {
            e.HasIndex(x => x.Token).IsUnique();
        });
    }
}
