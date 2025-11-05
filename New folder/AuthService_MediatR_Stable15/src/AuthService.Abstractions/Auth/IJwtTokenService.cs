using AuthService.Domain.Users;
namespace AuthService.Abstractions.Auth;

public interface IJwtTokenService
{
    (string accessToken, DateTime expiresUtc) CreateAccessToken(AppUser user, IEnumerable<string> roles);
    string CreateRefreshToken();
}
