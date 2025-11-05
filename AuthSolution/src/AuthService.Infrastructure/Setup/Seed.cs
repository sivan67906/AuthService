using AuthService.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure.Setup;

public static class Seed
{
    public static async Task ApplyAsync(IServiceProvider sp)
    {
        await using var scope = sp.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var write = provider.GetRequiredService<Persistence.WriteDbContext>();
        await write.Database.MigrateAsync();

        var roleMgr = provider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userMgr = provider.GetRequiredService<UserManager<AppUser>>();

        var roles = new[] { "Admin", "User", "Manager" };
        foreach (var r in roles)
            if (!await roleMgr.RoleExistsAsync(r))
                await roleMgr.CreateAsync(new IdentityRole<Guid>(r));

        var adminEmail = "admin@example.com";
        var admin = await userMgr.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = adminEmail,
                UserName = "admin",
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Admin"
            };
            await userMgr.CreateAsync(admin, "Admin#12345");
            await userMgr.AddToRoleAsync(admin, "Admin");
        }
    }
}
