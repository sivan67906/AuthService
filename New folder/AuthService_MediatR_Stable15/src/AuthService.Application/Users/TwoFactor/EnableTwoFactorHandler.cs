using AuthService.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
namespace AuthService.Application.Users.TwoFactor;

public sealed class EnableTwoFactorHandler(UserManager<AppUser> userManager) : IRequestHandler<EnableTwoFactorCommand, string?>
{
    public async Task<string?> Handle(EnableTwoFactorCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null) return null;
        var key = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key)) { await userManager.ResetAuthenticatorKeyAsync(user); key = await userManager.GetAuthenticatorKeyAsync(user); }
        user.TwoFactorEnabled = true; await userManager.UpdateAsync(user);
        return key;
    }
}
