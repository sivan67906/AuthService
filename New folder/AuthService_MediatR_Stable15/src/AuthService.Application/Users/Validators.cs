using FluentValidation;
using AuthService.Contracts.Users;
namespace AuthService.Application.Users;
public sealed class RegisterValidator : AbstractValidator<RegisterRequest>
{
    public RegisterValidator(){ RuleFor(x=>x.Email).NotEmpty().EmailAddress(); RuleFor(x=>x.Password).NotEmpty().MinimumLength(8); RuleFor(x=>x.FirstName).NotEmpty(); RuleFor(x=>x.LastName).NotEmpty(); }
}
public sealed class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator(){ RuleFor(x=>x.Email).NotEmpty().EmailAddress(); RuleFor(x=>x.Password).NotEmpty(); }
}
