using AuthService.Application.Abstractions;
using AuthService.Contracts;
using AuthService.Domain.Common;
using AuthService.Domain.Users;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Features.Auth;

public record LoginCommand(string Email, string Password, string? TwoFactorCode) : IRequest<Result<AuthDtos.LoginResponse>>;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginHandler : IRequestHandler<LoginCommand, Result<AuthDtos.LoginResponse>>
{
    private readonly IUserRepository _repo;
    private readonly ITokenService _tokenService;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public LoginHandler(IUserRepository repo, ITokenService tokenService, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _repo = repo;
        _tokenService = tokenService;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<Result<AuthDtos.LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var userRes = await _repo.FindByEmailAsync(request.Email, ct);
        if (!userRes.IsSuccess) return Result<AuthDtos.LoginResponse>.Failure(userRes.Error);
        var user = userRes.Value!;

        if (!user.EmailConfirmed)
            return Result<AuthDtos.LoginResponse>.Failure(new("auth.email_not_confirmed","Please confirm your email"));

        var pass = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!pass.Succeeded) return Result<AuthDtos.LoginResponse>.Failure(new("auth.invalid_credentials", "Invalid credentials"));

        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            if (string.IsNullOrWhiteSpace(request.TwoFactorCode))
                return Result<AuthDtos.LoginResponse>.Failure(new("auth.2fa_required","Two factor code required"));
            var valid2fa = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, request.TwoFactorCode);
            if (!valid2fa)
                return Result<AuthDtos.LoginResponse>.Failure(new("auth.2fa_invalid","Invalid two factor code"));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (access, exp) = _tokenService.CreateAccessToken(user, roles);
        var refresh = _tokenService.CreateRefreshToken();
        await _repo.AddRefreshTokenAsync(user.Id, new RefreshToken { Token = refresh, ExpiresAtUtc = DateTime.UtcNow.AddDays(14) }, ct);

        return Result<AuthDtos.LoginResponse>.Success(new(access, refresh, exp));
    }
}
