using AuthService.Application.Abstractions;
using AuthService.Domain.Users;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure.Setup;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<WriteDbContext>(opt =>
            opt.UseSqlServer(config.GetConnectionString("SqlServer")));

        services.AddDbContext<ReadDbContext>(opt =>
            opt.UseNpgsql(config.GetConnectionString("Postgres")));

        services.AddIdentityCore<AppUser>(o =>
        {
            o.User.RequireUniqueEmail = true;
            o.SignIn.RequireConfirmedEmail = true;
            o.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<WriteDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
