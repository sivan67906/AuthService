using AuthService.Domain.Common;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Authentication.Commands;

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                _logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
                return Result.Success();
            }

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Password reset requested for unconfirmed email: {Email}", request.Email);
                return Result.Success();
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"https://yourapp.com/reset-password?userId={user.Id}&token={Uri.EscapeDataString(resetToken)}";

            var emailResult = await _emailService.SendPasswordResetAsync(user.Email!, resetLink, cancellationToken);

            if (emailResult.IsFailure)
            {
                _logger.LogError("Failed to send password reset email to {Email}: {Error}",
                    request.Email, emailResult.Error?.Message);
            }
            else
            {
                _logger.LogInformation("Password reset email sent successfully to {Email}", request.Email);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during password reset request for {Email}", request.Email);
            return Result.Success();
        }
    }
}
