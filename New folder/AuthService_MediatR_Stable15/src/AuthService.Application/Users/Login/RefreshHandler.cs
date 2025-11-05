using AuthService.Abstractions.Auth;
using AuthService.Contracts.Users;
using AuthService.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
namespace AuthService.Application.Users.Login;

public sealed class RefreshHandler(UserManager<AppUser> userManager, IJwtTokenService jwt, IConfiguration cfg) : IRequestHandler<RefreshCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshCommand request, CancellationToken ct)
    {
        var user = userManager.Users.FirstOrDefault(u => u.RefreshToken == request.RefreshToken);
        if (user is null || user.RefreshTokenExpiresUtc is null || user.RefreshTokenExpiresUtc < DateTime.UtcNow) throw new UnauthorizedAccessException("Invalid refresh token");
        var roles = await userManager.GetRolesAsync(user);
        (string access, DateTime expires) = jwt.CreateAccessToken(user, roles);
        user.RefreshToken = jwt.CreateRefreshToken();
        user.RefreshTokenExpiresUtc = DateTime.UtcNow.AddDays(int.Parse(cfg["Jwt:RefreshTokenDays"]!));
        await userManager.UpdateAsync(user);
        return new AuthResponse(access, user.RefreshToken!, expires);
    }
}
