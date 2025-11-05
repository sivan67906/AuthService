using AuthService.Abstractions.Repositories;
using AuthService.Domain.Users;
using AuthService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace AuthService.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ReadDbContext _read;
    private readonly WriteDbContext _write;
    public UserRepository(UserManager<AppUser> userManager, ReadDbContext read, WriteDbContext write) { _userManager = userManager; _read = read; _write = write; }
    public Task<AppUser?> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        return _read.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public Task<AppUser?> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _read.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task AddAddressAsync(UserAddress address, CancellationToken ct = default) { _write.UserAddresses.Add(address); await _write.SaveChangesAsync(ct); }
    public Task<List<UserAddress>> GetAddressesAsync(Guid userId, CancellationToken ct = default)
    {
        return _read.UserAddresses.AsNoTracking().Where(a => a.UserId == userId).OrderByDescending(a => a.CreatedUtc).ToListAsync(ct);
    }
}
