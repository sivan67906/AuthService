using AuthService.Domain.Common;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Authentication.Commands;

public sealed class ResendConfirmationCommandHandler : IRequestHandler<ResendConfirmationCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<ResendConfirmationCommandHandler> _logger;

    public ResendConfirmationCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        ILogger<ResendConfirmationCommandHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> Handle(ResendConfirmationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                _logger.LogWarning("Resend confirmation requested for non-existent email: {Email}", request.Email);
                return Result.Success();
            }

            if (user.EmailConfirmed)
            {
                _logger.LogInformation("Resend confirmation requested for already verified email: {Email}", request.Email);
                return Result.Success();
            }

            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"https://yourapp.com/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(confirmationToken)}";

            var emailResult = await _emailService.SendEmailConfirmationAsync(user.Email!, confirmationLink, cancellationToken);

            if (emailResult.IsFailure)
            {
                _logger.LogError("Failed to send confirmation email to {Email}: {Error}",
                    request.Email, emailResult.Error?.Message);
            }
            else
            {
                _logger.LogInformation("Confirmation email resent successfully to {Email}", request.Email);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during resend confirmation for {Email}", request.Email);
            return Result.Success();
        }
    }
}
