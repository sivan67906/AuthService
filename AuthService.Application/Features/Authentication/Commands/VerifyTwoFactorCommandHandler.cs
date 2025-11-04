using AuthService.Domain.Common;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Authentication.Commands;

public sealed class VerifyTwoFactorCommandHandler : IRequestHandler<VerifyTwoFactorCommand, Result<LoginResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyTwoFactorCommandHandler> _logger;

    public VerifyTwoFactorCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITwoFactorService twoFactorService,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        ILogger<VerifyTwoFactorCommandHandler> logger)
    {
        _userManager = userManager;
        _twoFactorService = twoFactorService;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(VerifyTwoFactorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user is null)
            {
                _logger.LogWarning("2FA verification failed: User not found {UserId}", request.UserId);
                return Result<LoginResponse>.Failure(new Error("Auth.UserNotFound", "User not found"));
            }

            if (!user.TwoFactorEnabled || user.TwoFactorMethod == TwoFactorMethod.None)
            {
                _logger.LogWarning("2FA verification failed: 2FA not enabled for user {UserId}", user.Id);
                return Result<LoginResponse>.Failure(new Error("Auth.2FANotEnabled", "Two-factor authentication is not enabled"));
            }

            bool isCodeValid = false;

            switch (user.TwoFactorMethod)
            {
                case TwoFactorMethod.Authenticator:
                    if (string.IsNullOrEmpty(user.TwoFactorSecret))
                    {
                        _logger.LogError("2FA secret not found for user {UserId}", user.Id);
                        return Result<LoginResponse>.Failure(
                            new Error("Auth.2FANotConfigured", "Two-factor authentication is not properly configured"));
                    }
                    isCodeValid = _twoFactorService.ValidateCode(user.TwoFactorSecret, request.Code);
                    break;

                case TwoFactorMethod.Email:
                case TwoFactorMethod.SMS:
                    var provider = user.TwoFactorMethod == TwoFactorMethod.Email ? "Email" : "Phone";
                    isCodeValid = await _userManager.VerifyTwoFactorTokenAsync(user, provider, request.Code);
                    break;

                default:
                    _logger.LogWarning("Unknown 2FA method for user {UserId}: {Method}", user.Id, user.TwoFactorMethod);
                    return Result<LoginResponse>.Failure(
                        new Error("Auth.InvalidMethod", "Invalid two-factor authentication method"));
            }

            if (!isCodeValid)
            {
                _logger.LogWarning("Invalid 2FA code for user {UserId}", user.Id);
                return Result<LoginResponse>.Failure(
                    new Error("Auth.InvalidCode", "Invalid verification code"));
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

            _logger.LogInformation("User {UserId} logged in successfully with 2FA", user.Id);

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
            _logger.LogError(ex, "Error occurred during 2FA verification for user {UserId}", request.UserId);
            return Result<LoginResponse>.Failure(
                new Error("Auth.2FAError", "An error occurred during two-factor verification"));
        }
    }
}
