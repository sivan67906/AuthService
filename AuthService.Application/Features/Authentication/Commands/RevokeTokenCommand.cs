using AuthService.Domain.Common;
using FluentValidation;
using MediatR;

namespace AuthService.Application.Features.Authentication.Commands;

public sealed record RevokeTokenCommand(
    string RefreshToken,
    string? RevokedByIp = null
) : IRequest<Result>;

public sealed class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}
