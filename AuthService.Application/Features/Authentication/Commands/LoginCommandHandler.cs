using AuthService.Domain.Common;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Authentication.Commands;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find user by email or username
            var user = request.EmailOrUsername.Contains('@')
                ? await _userManager.FindByEmailAsync(request.EmailOrUsername)
                : await _userManager.FindByNameAsync(request.EmailOrUsername);

            if (user is null)
            {
                _logger.LogWarning("Login attempt failed: User not found for {EmailOrUsername}", request.EmailOrUsername);
                return Result<LoginResponse>.Failure(new Error("Auth.InvalidCredentials", "Invalid email/username or password"));
            }

            // Check if email is confirmed
            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Login attempt failed: Email not confirmed for user {UserId}", user.Id);
                return Result<LoginResponse>.Failure(new Error("Auth.EmailNotConfirmed", "Please confirm your email before logging in"));
            }

            // Check if user is active
            if (user.Status != UserStatus.Active)
            {
                _logger.LogWarning("Login attempt failed: User {UserId} is not active", user.Id);
                return Result<LoginResponse>.Failure(new Error("Auth.AccountInactive", "Your account is inactive. Please contact support"));
            }

            // Check password
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {UserId} is locked out", user.Id);
                return Result<LoginResponse>.Failure(new Error("Auth.AccountLockedOut", "Account is locked out due to multiple failed attempts"));
            }

            if (result.IsNotAllowed)
            {
                _logger.LogWarning("Login not allowed for user {UserId}", user.Id);
                return Result<LoginResponse>.Failure(new Error("Auth.LoginNotAllowed", "Login is not allowed for this account"));
            }

            if (!result.Succeeded)
            {
                _logger.LogWarning("Invalid password attempt for user {UserId}", user.Id);
                return Result<LoginResponse>.Failure(new Error("Auth.InvalidCredentials", "Invalid email/username or password"));
            }

            // Check if two-factor is required
            if (user.TwoFactorEnabled && user.TwoFactorMethod != TwoFactorMethod.None)
            {
                // TODO: Send two-factor code
                _logger.LogInformation("Two-factor authentication required for user {UserId}", user.Id);
                
                return Result<LoginResponse>.Success(new LoginResponse(
                    string.Empty,
                    string.Empty,
                    DateTime.UtcNow,
                    user.Id,
                    user.Email!,
                    user.UserName!,
                    Array.Empty<string>(),
                    RequiresTwoFactor: true,
                    TwoFactorMethod: user.TwoFactorMethod.ToString()
                ));
            }

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate JWT token
            var accessToken = _jwtService.GenerateAccessToken(
                user.Id,
                user.Email!,
                user.UserName!,
                roles,
                Array.Empty<string>()
            );

            // Generate and store refresh token
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

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = request.IpAddress;
            await _userManager.UpdateAsync(user);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} logged in successfully", user.Id);

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
            _logger.LogError(ex, "Error occurred during login for {EmailOrUsername}", request.EmailOrUsername);
            return Result<LoginResponse>.Failure(new Error("Auth.LoginError", "An error occurred during login"));
        }
    }
}
