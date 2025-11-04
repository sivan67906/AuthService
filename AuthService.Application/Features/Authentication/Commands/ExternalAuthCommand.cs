using AuthService.Domain.Common;
using FluentValidation;
using MediatR;

namespace AuthService.Application.Features.Authentication.Commands;

public sealed record ExternalAuthCommand(
    string Provider,
    string Token,
    string? DeviceInfo = null,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<Result<LoginResponse>>;

public sealed class ExternalAuthCommandValidator : AbstractValidator<ExternalAuthCommand>
{
    public ExternalAuthCommandValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required")
            .Must(provider => provider is "Google" or "Microsoft")
            .WithMessage("Provider must be 'Google' or 'Microsoft'");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required");
    }
}
