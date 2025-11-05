using AuthService.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
namespace AuthService.Application.Users.Password;

public sealed class ChangePasswordHandler(UserManager<AppUser> userManager) : IRequestHandler<ChangePasswordCommand, bool>
{
    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null) return false;
        var res = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        return res.Succeeded;
    }
}
