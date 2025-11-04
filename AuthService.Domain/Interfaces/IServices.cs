using AuthService.Domain.Common;

namespace AuthService.Domain.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(Guid userId, string email, string username, IEnumerable<string> roles, IEnumerable<string> claims);
    string GenerateRefreshToken();
    string GetJwtIdFromToken(string token);
    bool ValidateToken(string token);
    Task<Result<string>> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}

public interface IEmailService
{
    Task<Result> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task<Result> SendEmailConfirmationAsync(string email, string confirmationLink, CancellationToken cancellationToken = default);
    Task<Result> SendPasswordResetAsync(string email, string resetLink, CancellationToken cancellationToken = default);
    Task<Result> SendTwoFactorCodeAsync(string email, string code, CancellationToken cancellationToken = default);
}

public interface ISmsService
{
    Task<Result> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
    Task<Result> SendTwoFactorCodeAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
}

public interface ITwoFactorService
{
    string GenerateSecret();
    string GenerateCode(string secret);
    bool ValidateCode(string secret, string code);
    string GenerateQrCodeUri(string email, string secret, string issuer = "AuthService");
}

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public interface IExternalAuthService
{
    Task<Result<ExternalAuthResult>> AuthenticateGoogleAsync(string idToken, CancellationToken cancellationToken = default);
    Task<Result<ExternalAuthResult>> AuthenticateMicrosoftAsync(string accessToken, CancellationToken cancellationToken = default);
}

public sealed record ExternalAuthResult(
    string Email,
    string FirstName,
    string LastName,
    string Provider,
    string ProviderKey,
    string? ProfilePictureUrl = null
);
