using AuthService.Domain.Common;
using FluentValidation;
using MediatR;

namespace AuthService.Application.Features.Authentication.Commands;

public sealed record ResendConfirmationCommand(
    string Email
) : IRequest<Result>;

public sealed class ResendConfirmationCommandValidator : AbstractValidator<ResendConfirmationCommand>
{
    public ResendConfirmationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}
