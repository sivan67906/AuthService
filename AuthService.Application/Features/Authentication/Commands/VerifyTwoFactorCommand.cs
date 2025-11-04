using AuthService.Domain.Common;
using FluentValidation;
using MediatR;

namespace AuthService.Application.Features.Authentication.Commands;

public sealed record VerifyTwoFactorCommand(
    Guid UserId,
    string Code,
    string? DeviceInfo = null,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<Result<LoginResponse>>;

public sealed class VerifyTwoFactorCommandValidator : AbstractValidator<VerifyTwoFactorCommand>
{
    public VerifyTwoFactorCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required")
            .Length(6).WithMessage("Verification code must be 6 digits")
            .Matches("^[0-9]{6}$").WithMessage("Verification code must contain only numbers");
    }
}
