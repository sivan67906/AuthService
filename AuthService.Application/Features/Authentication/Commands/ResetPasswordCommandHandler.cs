using AuthService.Domain.Common;
using AuthService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Authentication.Commands;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user is null)
            {
                _logger.LogWarning("Password reset failed: User not found {UserId}", request.UserId);
                return Result.Failure(new Error("Auth.UserNotFound", "User not found"));
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Password reset failed for user {UserId}: {Errors}", user.Id, errors);
                return Result.Failure(new Error("Auth.ResetFailed", errors));
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Password reset successfully for user {UserId}", user.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during password reset for user {UserId}", request.UserId);
            return Result.Failure(new Error("Auth.ResetError", "An error occurred during password reset"));
        }
    }
}
