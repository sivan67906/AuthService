using AuthService.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace AuthService.Infrastructure.Persistence;
public sealed class WriteDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public WriteDbContext(DbContextOptions<WriteDbContext> options) : base(options) { }
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<AppUser>(e => { e.Property(x=>x.FirstName).HasMaxLength(200); e.Property(x=>x.LastName).HasMaxLength(200); });
        builder.Entity<UserAddress>(e => {
            e.HasKey(x=>x.Id);
            e.Property(x=>x.Line1).HasMaxLength(512).IsRequired();
            e.Property(x=>x.City).HasMaxLength(128).IsRequired();
            e.Property(x=>x.State).HasMaxLength(128).IsRequired();
            e.Property(x=>x.PostalCode).HasMaxLength(32).IsRequired();
            e.Property(x=>x.Country).HasMaxLength(128).IsRequired();
            e.HasOne(x=>x.User).WithMany(x=>x.Addresses).HasForeignKey(x=>x.UserId);
        });
    }
}
