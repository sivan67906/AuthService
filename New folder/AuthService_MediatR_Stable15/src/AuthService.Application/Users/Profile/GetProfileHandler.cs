using AuthService.Contracts.Users;
using AuthService.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Users.Profile;

public sealed class GetProfileHandler(UserManager<AppUser> userManager) : IRequestHandler<GetProfileQuery, ProfileResponse>
{
    public async Task<ProfileResponse> Handle(GetProfileQuery request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null) throw new KeyNotFoundException("User not found");
        var roles = await userManager.GetRolesAsync(user);
        return new ProfileResponse(user.Id, user.Email!, user.FirstName, user.LastName, roles.ToArray(), user.EmailConfirmed, user.TwoFactorEnabled);
    }
}
