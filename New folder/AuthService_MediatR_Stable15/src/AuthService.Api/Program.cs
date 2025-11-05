using Microsoft.AspNetCore.Authentication;
using System.Text;
using AuthService.Application;
using AuthService.Infrastructure;
using AuthService.Infrastructure.Auth;
using AuthService.Domain.Users;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using AuthService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddApplication().AddInfrastructure(builder.Configuration);

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(o => { 
        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidateAudience = true, ValidateIssuerSigningKey = true, ValidateLifetime = true,
            ValidIssuer = jwtSection["Issuer"], ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    })
    .AddGoogle(o => { o.ClientId = builder.Configuration["ExternalAuth:Google:ClientId"]!; o.ClientSecret = builder.Configuration["ExternalAuth:Google:ClientSecret"]!; })
    .AddMicrosoftAccount(o => { o.ClientId = builder.Configuration["ExternalAuth:Microsoft:ClientId"]!; o.ClientSecret = builder.Configuration["ExternalAuth:Microsoft:ClientSecret"]!; });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

var app = builder.Build();

if (app.Environment.IsDevelopment()){ app.UseSwagger(); app.UseSwaggerUI(); }
app.UseSerilogRequestLogging();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var write = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
    await write.Database.MigrateAsync();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    string[] roles = ["Admin", "User"];
    foreach (var r in roles) if (!await roleManager.RoleExistsAsync(r)) await roleManager.CreateAsync(new IdentityRole<Guid>(r));
    var adminEmail = "admin@demo.local";
    var admin = await userManager.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
    if (admin is null){ admin = new AppUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, FirstName = "Admin", LastName = "User" }; await userManager.CreateAsync(admin, "Admin@12345"); await userManager.AddToRolesAsync(admin, roles); }
}

app.Run();
public partial class Program { }
