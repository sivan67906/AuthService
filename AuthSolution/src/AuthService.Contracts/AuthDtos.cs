namespace AuthService.Contracts;

public static class AuthDtos
{
    public record RegisterRequest(string Email, string Password, string FirstName, string LastName);
    public record LoginRequest(string Email, string Password, string? TwoFactorCode = null);
    public record LoginResponse(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc);
    public record RefreshTokenRequest(string RefreshToken);
    public record RefreshTokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc);
    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Email, string Token, string NewPassword);
    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
    public record AddressRequest(string Line1, string? Line2, string City, string State, string Country, string PostalCode);
    public record ProfileResponse(Guid Id, string Email, string? FirstName, string? LastName, IEnumerable<AddressDto> Addresses, IEnumerable<string> Roles);
    public record AddressDto(Guid Id, string Line1, string? Line2, string City, string State, string Country, string PostalCode, DateTime CreatedAtUtc);
    public record ExternalLoginRequest(string Provider, string IdToken);
}
