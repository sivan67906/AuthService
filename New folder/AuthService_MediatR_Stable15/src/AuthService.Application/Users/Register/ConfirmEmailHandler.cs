using AuthService.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
namespace AuthService.Application.Users.Register;

public sealed class ConfirmEmailHandler(UserManager<AppUser> userManager) : IRequestHandler<ConfirmEmailCommand, bool>
{
    public async Task<bool> Handle(ConfirmEmailCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null) return false;
        var res = await userManager.ConfirmEmailAsync(user, request.Token);
        return res.Succeeded;
    }
}
