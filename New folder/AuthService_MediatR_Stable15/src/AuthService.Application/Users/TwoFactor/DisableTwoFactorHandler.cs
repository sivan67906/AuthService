using AuthService.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
namespace AuthService.Application.Users.TwoFactor;

public sealed class DisableTwoFactorHandler(UserManager<AppUser> userManager) : IRequestHandler<DisableTwoFactorCommand, bool>
{
    public async Task<bool> Handle(DisableTwoFactorCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null) return false;
        user.TwoFactorEnabled = false; await userManager.UpdateAsync(user);
        return true;
    }
}
