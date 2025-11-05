using AuthService.Domain.Users;
using AuthService.Domain.Common;

namespace AuthService.Application.Abstractions;

public interface IUserRepository
{
    Task<Result<AppUser>> CreateUserAsync(AppUser user, string password, CancellationToken ct);
    Task<Result<AppUser>> FindByEmailAsync(string email, CancellationToken ct);
    Task<Result> AddAddressAsync(Guid userId, Address address, CancellationToken ct);
    Task<Result<IReadOnlyList<Address>>> GetAddressesAsync(Guid userId, CancellationToken ct);
    Task<Result> AddRefreshTokenAsync(Guid userId, RefreshToken token, CancellationToken ct);
    Task<Result<RefreshToken>> GetRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken ct);
    Task<Result> RevokeRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken ct);
}
