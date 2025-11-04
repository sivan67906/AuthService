using AuthService.Domain.Common;
using FluentValidation;
using MediatR;

namespace AuthService.Application.Features.Authentication.Commands;

public sealed record EnableTwoFactorCommand(
    Guid UserId,
    string Method
) : IRequest<Result<EnableTwoFactorResponse>>;

public sealed record EnableTwoFactorResponse(
    bool Success,
    string Method,
    string? Secret = null,
    string? QrCodeUri = null,
    string Message = "Two-factor authentication enabled successfully"
);

public sealed class EnableTwoFactorCommandValidator : AbstractValidator<EnableTwoFactorCommand>
{
    public EnableTwoFactorCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Method)
            .NotEmpty().WithMessage("Two-factor method is required")
            .Must(method => method is "Email" or "SMS" or "Authenticator")
            .WithMessage("Method must be 'Email', 'SMS', or 'Authenticator'");
    }
}
