using AuthService.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
namespace AuthService.Application.Users.Login;

public sealed class RevokeHandler(UserManager<AppUser> userManager) : IRequestHandler<RevokeCommand, bool>
{
    public async Task<bool> Handle(RevokeCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null) return false;
        user.RefreshToken = null; user.RefreshTokenExpiresUtc = null;
        await userManager.UpdateAsync(user);
        return true;
    }
}
