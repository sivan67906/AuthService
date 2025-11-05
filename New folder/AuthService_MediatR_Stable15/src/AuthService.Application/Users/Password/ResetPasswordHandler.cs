using AuthService.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
namespace AuthService.Application.Users.Password;

public sealed class ResetPasswordHandler(UserManager<AppUser> userManager) : IRequestHandler<ResetPasswordCommand, bool>
{
    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null) return false;
        var res = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        return res.Succeeded;
    }
}
