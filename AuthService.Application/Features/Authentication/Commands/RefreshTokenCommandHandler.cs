using AuthService.Domain.Common;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Authentication.Commands;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var storedToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken);

            if (storedToken is null)
            {
                _logger.LogWarning("Refresh token not found: {Token}", request.RefreshToken);
                return Result<LoginResponse>.Failure(new Error("Auth.InvalidToken", "Invalid refresh token"));
            }

            if (storedToken.IsUsed)
            {
                _logger.LogWarning("Refresh token already used: {TokenId}", storedToken.Id);
                return Result<LoginResponse>.Failure(new Error("Auth.TokenUsed", "Refresh token has already been used"));
            }

            if (storedToken.IsRevoked)
            {
                _logger.LogWarning("Refresh token revoked: {TokenId}", storedToken.Id);
                return Result<LoginResponse>.Failure(new Error("Auth.TokenRevoked", "Refresh token has been revoked"));
            }

            if (storedToken.IsExpired)
            {
                _logger.LogWarning("Refresh token expired: {TokenId}", storedToken.Id);
                return Result<LoginResponse>.Failure(new Error("Auth.TokenExpired", "Refresh token has expired"));
            }

            var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
            if (user is null)
            {
                _logger.LogError("User not found for refresh token: {UserId}", storedToken.UserId);
                return Result<LoginResponse>.Failure(new Error("Auth.UserNotFound", "User not found"));
            }

            var roles = await _userManager.GetRolesAsync(user);

            var newAccessToken = _jwtService.GenerateAccessToken(
                user.Id,
                user.Email!,
                user.UserName!,
                roles,
                Array.Empty<string>()
            );

            var newRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = _jwtService.GenerateRefreshToken(),
                JwtId = _jwtService.GetJwtIdFromToken(newAccessToken),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                DeviceInfo = storedToken.DeviceInfo,
                IpAddress = storedToken.IpAddress,
                UserAgent = storedToken.UserAgent,
                CreatedAt = DateTime.UtcNow
            };

            storedToken.IsUsed = true;
            storedToken.ReplacedByToken = newRefreshToken.Token;
            _unitOfWork.RefreshTokens.Update(storedToken);

            await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Refresh token successfully refreshed for user {UserId}", user.Id);

            return Result<LoginResponse>.Success(new LoginResponse(
                newAccessToken,
                newRefreshToken.Token,
                newRefreshToken.ExpiresAt,
                user.Id,
                user.Email!,
                user.UserName!,
                roles
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during token refresh");
            return Result<LoginResponse>.Failure(new Error("Auth.RefreshError", "An error occurred during token refresh"));
        }
    }
}
