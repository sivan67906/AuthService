using AuthService.Domain.Users;

namespace AuthService.Application.Abstractions;

public interface ITokenService
{
    (string accessToken, DateTime expiresAtUtc) CreateAccessToken(AppUser user, IEnumerable<string> roles);
    string CreateRefreshToken();
}
