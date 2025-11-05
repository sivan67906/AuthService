using AuthService.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence;

public class ReadDbContext : DbContext
{
    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options) { }
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Address> Addresses => Set<Address>();
}
