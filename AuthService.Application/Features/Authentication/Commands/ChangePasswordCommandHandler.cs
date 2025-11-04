using AuthService.Domain.Common;
using AuthService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Authentication.Commands;

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user is null)
            {
                _logger.LogWarning("Change password failed: User not found {UserId}", request.UserId);
                return Result.Failure(new Error("Auth.UserNotFound", "User not found"));
            }

            var result = await _userManager.ChangePasswordAsync(
                user,
                request.CurrentPassword,
                request.NewPassword
            );

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Change password failed for user {UserId}: {Errors}", user.Id, errors);

                var incorrectPasswordError = result.Errors.FirstOrDefault(e => e.Code == "PasswordMismatch");
                if (incorrectPasswordError is not null)
                {
                    return Result.Failure(new Error("Auth.IncorrectPassword", "Current password is incorrect"));
                }

                return Result.Failure(new Error("Auth.ChangePasswordFailed", errors));
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Password changed successfully for user {UserId}", user.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during password change for user {UserId}", request.UserId);
            return Result.Failure(new Error("Auth.ChangePasswordError", "An error occurred during password change"));
        }
    }
}
