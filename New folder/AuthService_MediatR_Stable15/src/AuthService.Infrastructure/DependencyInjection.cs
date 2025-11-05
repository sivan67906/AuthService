using AuthService.Abstractions.Auth;
using AuthService.Abstractions.Repositories;
using AuthService.Domain.Users;
using AuthService.Infrastructure.Auth;
using AuthService.Infrastructure.Email;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace AuthService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContexts
        services.AddDbContext<WriteDbContext>(opt =>
            opt.UseSqlServer(configuration.GetConnectionString("SqlServer")));

        services.AddDbContext<ReadDbContext>(opt =>
            opt.UseNpgsql(configuration.GetConnectionString("Postgres")));

        // Identity
        var identityBuilder = services
            .AddIdentityCore<AppUser>(opt =>
            {
                opt.SignIn.RequireConfirmedEmail = true;
                opt.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
                opt.Password.RequiredLength = 8;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<WriteDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        // Auth, Repositories, Http
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<AuthService.Abstractions.Email.IEmailSender, EmailSender>();

        services.AddHttpClient("email", c =>
        {
            c.BaseAddress = new Uri("https://example-email.local");
        });

        // HealthChecks
        services.AddHealthChecks();

        // Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        return services;
    }
}
