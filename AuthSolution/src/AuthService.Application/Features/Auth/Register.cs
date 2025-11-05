using AuthService.Application.Abstractions;
using AuthService.Contracts;
using AuthService.Domain.Common;
using AuthService.Domain.Users;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Features.Auth;

public record RegisterCommand(string Email, string Password, string FirstName, string LastName) : IRequest<Result<Guid>>;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
    }
}

public class RegisterHandler : IRequestHandler<RegisterCommand, Result<Guid>>
{
    private readonly IUserRepository _repo;
    private readonly UserManager<AppUser> _userManager;

    public RegisterHandler(IUserRepository repo, UserManager<AppUser> userManager)
    {
        _repo = repo;
        _userManager = userManager;
    }

    public async Task<Result<Guid>> Handle(RegisterCommand request, CancellationToken ct)
    {
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var created = await _repo.CreateUserAsync(user, request.Password, ct);
        if (!created.IsSuccess) return Result<Guid>.Failure(created.Error);

        // Assign default role
        await _userManager.AddToRoleAsync(user, "User");
        return Result<Guid>.Success(user.Id);
    }
}
