using AuthService.Domain.Common;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Authentication.Commands;

public sealed class EnableTwoFactorCommandHandler : IRequestHandler<EnableTwoFactorCommand, Result<EnableTwoFactorResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITwoFactorService _twoFactorService;
    private readonly ILogger<EnableTwoFactorCommandHandler> _logger;

    public EnableTwoFactorCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITwoFactorService twoFactorService,
        ILogger<EnableTwoFactorCommandHandler> logger)
    {
        _userManager = userManager;
        _twoFactorService = twoFactorService;
        _logger = logger;
    }

    public async Task<Result<EnableTwoFactorResponse>> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user is null)
            {
                _logger.LogWarning("Enable 2FA failed: User not found {UserId}", request.UserId);
                return Result<EnableTwoFactorResponse>.Failure(new Error("Auth.UserNotFound", "User not found"));
            }

            TwoFactorMethod method = request.Method switch
            {
                "Email" => TwoFactorMethod.Email,
                "SMS" => TwoFactorMethod.SMS,
                "Authenticator" => TwoFactorMethod.Authenticator,
                _ => TwoFactorMethod.None
            };

            if (method == TwoFactorMethod.None)
            {
                return Result<EnableTwoFactorResponse>.Failure(
                    new Error("Auth.InvalidMethod", "Invalid two-factor method"));
            }

            user.TwoFactorEnabled = true;
            user.TwoFactorMethod = method;
            user.UpdatedAt = DateTime.UtcNow;

            EnableTwoFactorResponse response;

            if (method == TwoFactorMethod.Authenticator)
            {
                var secret = _twoFactorService.GenerateSecret();
                var qrCodeUri = _twoFactorService.GenerateQrCodeUri(user.Email!, secret);

                user.TwoFactorSecret = secret;

                response = new EnableTwoFactorResponse(
                    Success: true,
                    Method: method.ToString(),
                    Secret: secret,
                    QrCodeUri: qrCodeUri,
                    Message: "Scan the QR code with your authenticator app and verify the code to complete setup"
                );
            }
            else
            {
                response = new EnableTwoFactorResponse(
                    Success: true,
                    Method: method.ToString(),
                    Message: $"Two-factor authentication via {method} has been enabled"
                );
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to enable 2FA for user {UserId}: {Errors}", user.Id, errors);
                return Result<EnableTwoFactorResponse>.Failure(
                    new Error("Auth.Enable2FAFailed", errors));
            }

            _logger.LogInformation("Two-factor authentication ({Method}) enabled for user {UserId}",
                method, user.Id);

            return Result<EnableTwoFactorResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while enabling 2FA for user {UserId}", request.UserId);
            return Result<EnableTwoFactorResponse>.Failure(
                new Error("Auth.Enable2FAError", "An error occurred while enabling two-factor authentication"));
        }
    }
}
