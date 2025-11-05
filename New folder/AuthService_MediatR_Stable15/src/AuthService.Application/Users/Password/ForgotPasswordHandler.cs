using AuthService.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
namespace AuthService.Application.Users.Password;

public sealed class ForgotPasswordHandler(UserManager<AppUser> userManager) : IRequestHandler<ForgotPasswordCommand, string?>
{
    public async Task<string?> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null) return null;
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return token;
    }
}
