using AuthService.Domain.Users;
using Microsoft.EntityFrameworkCore;
namespace AuthService.Infrastructure.Persistence;

public sealed class ReadDbContext : DbContext
{
    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options) { }
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>().ToTable("AspNetUsers");
        modelBuilder.Entity<UserAddress>().ToTable("UserAddresses");
    }
}
