using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces;

public interface IUserRepository : IRepository<ApplicationUser>
{
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetWithAddressesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetWithRolesAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IAddressRepository : IRepository<Address>
{
    Task<IReadOnlyList<Address>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Address?> GetDefaultAddressAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SetDefaultAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);
}

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task RevokeAsync(string token, string? revokedByIp = null, CancellationToken cancellationToken = default);
    Task RevokeAllByUserIdAsync(Guid userId, string? revokedByIp = null, CancellationToken cancellationToken = default);
    Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}

public interface IExternalLoginRepository : IRepository<ExternalLogin>
{
    Task<ExternalLogin?> GetByProviderAsync(
        string provider,
        string providerKey,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExternalLogin>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IUserSessionRepository : IRepository<UserSession>
{
    Task<IReadOnlyList<UserSession>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserSession?> GetByTokenAsync(string sessionToken, CancellationToken cancellationToken = default);
    Task EndSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task EndAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
