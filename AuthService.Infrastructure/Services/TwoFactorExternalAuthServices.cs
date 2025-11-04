using AuthService.Domain.Common;
using AuthService.Domain.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using OtpNet;
using QRCoder;

namespace AuthService.Infrastructure.Services;

public sealed class TwoFactorService : ITwoFactorService
{
    public string GenerateSecret()
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    public string GenerateCode(string secret)
    {
        var key = Base32Encoding.ToBytes(secret);
        var totp = new Totp(key);
        return totp.ComputeTotp();
    }

    public bool ValidateCode(string secret, string code)
    {
        try
        {
            var key = Base32Encoding.ToBytes(secret);
            var totp = new Totp(key);
            long timeStepMatched;
            return totp.VerifyTotp(code, out timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay);
        }
        catch
        {
            return false;
        }
    }

    public string GenerateQrCodeUri(string email, string secret, string issuer = "AuthService")
    {
        return $"otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}";
    }
}

public sealed class ExternalAuthService : IExternalAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExternalAuthService> _logger;

    public ExternalAuthService(IConfiguration configuration, ILogger<ExternalAuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<ExternalAuthResult>> AuthenticateGoogleAsync(string idToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _configuration["Authentication:Google:ClientId"] ?? "" }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            if (payload is null)
            {
                return Result<ExternalAuthResult>.Failure(new Error("ExternalAuth.GoogleFailed", "Invalid Google token"));
            }

            var result = new ExternalAuthResult(
                payload.Email,
                payload.GivenName ?? "",
                payload.FamilyName ?? "",
                "Google",
                payload.Subject,
                payload.Picture
            );

            _logger.LogInformation("Google authentication successful for {Email}", payload.Email);
            return Result<ExternalAuthResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google authentication failed");
            return Result<ExternalAuthResult>.Failure(new Error("ExternalAuth.GoogleError", ex.Message));
        }
    }

    public async Task<Result<ExternalAuthResult>> AuthenticateMicrosoftAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: Implement Microsoft authentication validation
            // This is a placeholder implementation
            _logger.LogInformation("Microsoft authentication attempted");
            
            // For now, return a failure as this needs actual Microsoft Graph API integration
            await Task.Delay(100, cancellationToken);
            
            return Result<ExternalAuthResult>.Failure(
                new Error("ExternalAuth.NotImplemented", "Microsoft authentication is not yet implemented"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Microsoft authentication failed");
            return Result<ExternalAuthResult>.Failure(new Error("ExternalAuth.MicrosoftError", ex.Message));
        }
    }
}
