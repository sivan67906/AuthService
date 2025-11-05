using AuthService.Abstractions.Email;
using AuthService.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Users.Register;

public sealed class RegisterHandler(UserManager<AppUser> userManager, IEmailSender email, LinkGeneratorAccessor links) : IRequestHandler<RegisterCommand, RegisterResult>
{
    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken ct)
    {
        var r = request.Request;
        var exists = await userManager.FindByEmailAsync(request.Request.Email) is not null;
        if (exists) throw new InvalidOperationException("Email already in use.");
        var user = new AppUser { UserName = r.Email, Email = r.Email, FirstName = r.FirstName, LastName = r.LastName };
        var res = await userManager.CreateAsync(user, r.Password);
        if (!res.Succeeded) throw new InvalidOperationException(string.Join(";", res.Errors.Select(e => e.Description)));
        await userManager.AddToRoleAsync(user, "User");
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmUrl = links.BuildConfirmEmailUrl(user.Id, token);
        await email.SendAsync(user.Email!, "Confirm your email", $"<p>Click to confirm: <a href='{confirmUrl}'>Confirm</a></p>", ct);
        return new RegisterResult(user.Id, user.Email!, token);
    }
}
