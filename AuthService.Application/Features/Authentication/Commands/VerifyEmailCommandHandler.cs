using AuthService.Domain.Common;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Authentication.Commands;

public sealed class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<VerifyEmailCommandHandler> _logger;

    public VerifyEmailCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<VerifyEmailCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user is null)
            {
                _logger.LogWarning("Email verification failed: User not found {UserId}", request.UserId);
                return Result.Failure(new Error("Auth.UserNotFound", "User not found"));
            }

            if (user.EmailConfirmed)
            {
                _logger.LogInformation("Email already verified for user {UserId}", user.Id);
                return Result.Failure(new Error("Auth.EmailAlreadyVerified", "Email is already verified"));
            }

            var result = await _userManager.ConfirmEmailAsync(user, request.Token);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Email verification failed for user {UserId}: {Errors}", user.Id, errors);
                return Result.Failure(new Error("Auth.VerificationFailed", "Email verification failed. Token may be invalid or expired."));
            }

            user.IsEmailVerified = true;
            user.EmailVerifiedAt = DateTime.UtcNow;
            user.Status = UserStatus.Active;
            user.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Email verified successfully for user {UserId}", user.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during email verification for user {UserId}", request.UserId);
            return Result.Failure(new Error("Auth.VerificationError", "An error occurred during email verification"));
        }
    }
}
