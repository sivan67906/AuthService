using AuthService.Domain.Common;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Authentication.Commands;

public sealed class ExternalAuthCommandHandler : IRequestHandler<ExternalAuthCommand, Result<LoginResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IExternalAuthService _externalAuthService;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExternalAuthCommandHandler> _logger;

    public ExternalAuthCommandHandler(
        UserManager<ApplicationUser> userManager,
        IExternalAuthService externalAuthService,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        ILogger<ExternalAuthCommandHandler> logger)
    {
        _userManager = userManager;
        _externalAuthService = externalAuthService;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(ExternalAuthCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Result<ExternalAuthResult> authResult = request.Provider switch
            {
                "Google" => await _externalAuthService.AuthenticateGoogleAsync(request.Token, cancellationToken),
                "Microsoft" => await _externalAuthService.AuthenticateMicrosoftAsync(request.Token, cancellationToken),
                _ => Result<ExternalAuthResult>.Failure(new Error("Auth.InvalidProvider", "Invalid provider"))
            };

            if (authResult.IsFailure || authResult.Value is null)
            {
                _logger.LogWarning("External authentication failed for provider {Provider}", request.Provider);
                return Result<LoginResponse>.Failure(
                    authResult.Error ?? new Error("Auth.ExternalAuthFailed", "External authentication failed"));
            }

            var externalAuth = authResult.Value;

            var user = await _userManager.FindByEmailAsync(externalAuth.Email);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    Email = externalAuth.Email,
                    UserName = externalAuth.Email,
                    FirstName = externalAuth.FirstName,
                    LastName = externalAuth.LastName,
                    ProfilePictureUrl = externalAuth.ProfilePictureUrl,
                    EmailConfirmed = true,
                    IsEmailVerified = true,
                    EmailVerifiedAt = DateTime.UtcNow,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create user from external auth: {Errors}", errors);
                    return Result<LoginResponse>.Failure(
                        new Error("Auth.UserCreationFailed", $"Failed to create user: {errors}"));
                }

                await _userManager.AddToRoleAsync(user, "Customer");

                _logger.LogInformation("New user created from external auth: {UserId}, Provider: {Provider}",
                    user.Id, request.Provider);
            }

            var existingExternalLogin = await _unitOfWork.Repository<ExternalLogin>()
                .FirstOrDefaultAsync(el => el.UserId == user.Id && el.Provider == externalAuth.Provider, cancellationToken);

            if (existingExternalLogin is null)
            {
                var externalLogin = new ExternalLogin
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Provider = externalAuth.Provider,
                    ProviderKey = externalAuth.ProviderKey,
                    ProviderDisplayName = externalAuth.Provider,
                    Email = externalAuth.Email,
                    LastLoginAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<ExternalLogin>().AddAsync(externalLogin, cancellationToken);
            }
            else
            {
                existingExternalLogin.LastLoginAt = DateTime.UtcNow;
                _unitOfWork.Repository<ExternalLogin>().Update(existingExternalLogin);
            }

            var roles = await _userManager.GetRolesAsync(user);

            var accessToken = _jwtService.GenerateAccessToken(
                user.Id,
                user.Email!,
                user.UserName!,
                roles,
                Array.Empty<string>()
            );

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = _jwtService.GenerateRefreshToken(),
                JwtId = _jwtService.GetJwtIdFromToken(accessToken),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                DeviceInfo = request.DeviceInfo,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);

            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = request.IpAddress;
            await _userManager.UpdateAsync(user);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} logged in successfully via {Provider}", user.Id, request.Provider);

            return Result<LoginResponse>.Success(new LoginResponse(
                accessToken,
                refreshToken.Token,
                refreshToken.ExpiresAt,
                user.Id,
                user.Email!,
                user.UserName!,
                roles
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during external authentication for provider {Provider}", request.Provider);
            return Result<LoginResponse>.Failure(
                new Error("Auth.ExternalAuthError", "An error occurred during external authentication"));
        }
    }
}
