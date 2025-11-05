using AuthService.Domain.Users;
namespace AuthService.Abstractions.Repositories;

public interface IUserRepository
{
    Task<AppUser?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<AppUser?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAddressAsync(UserAddress address, CancellationToken ct = default);
    Task<List<UserAddress>> GetAddressesAsync(Guid userId, CancellationToken ct = default);
}
