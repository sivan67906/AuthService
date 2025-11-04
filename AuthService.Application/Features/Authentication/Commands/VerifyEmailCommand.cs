using AuthService.Domain.Common;
using FluentValidation;
using MediatR;

namespace AuthService.Application.Features.Authentication.Commands;

public sealed record VerifyEmailCommand(
    Guid UserId,
    string Token
) : IRequest<Result>;

public sealed class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Verification token is required");
    }
}
