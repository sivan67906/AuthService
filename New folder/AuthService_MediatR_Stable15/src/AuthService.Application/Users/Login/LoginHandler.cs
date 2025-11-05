using AuthService.Abstractions.Auth;
using AuthService.Contracts.Users;
using AuthService.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
namespace AuthService.Application.Users.Login;

public sealed class LoginHandler(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IJwtTokenService jwt, IConfiguration cfg) : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        var r = request.Request;
        var user = await userManager.FindByEmailAsync(r.Email);
        if (user is null) throw new UnauthorizedAccessException("Invalid credentials");
        if (!user.EmailConfirmed) throw new UnauthorizedAccessException("Email not confirmed");
        var pw = await signInManager.CheckPasswordSignInAsync(user, r.Password, lockoutOnFailure: true);
        if (!pw.Succeeded) throw new UnauthorizedAccessException("Invalid credentials");
        if (user.TwoFactorEnabled)
        {
            if (string.IsNullOrWhiteSpace(r.TwoFactorCode)) throw new UnauthorizedAccessException("2FA code required");
            var valid2fa = await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, r.TwoFactorCode);
            if (!valid2fa) throw new UnauthorizedAccessException("Invalid 2FA code");
        }
        var roles = await userManager.GetRolesAsync(user);
        (string access, DateTime expires) = jwt.CreateAccessToken(user, roles);
        var refresh = jwt.CreateRefreshToken();
        user.RefreshToken = refresh;
        user.RefreshTokenExpiresUtc = DateTime.UtcNow.AddDays(int.Parse(cfg["Jwt:RefreshTokenDays"]!));
        await userManager.UpdateAsync(user);
        return new AuthResponse(access, refresh, expires);
    }
}
