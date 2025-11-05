using AuthService.Domain.Common;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Authentication.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    UserManager<ApplicationUser> userManager,
    IJwtService jwtService,
    ILogger<RefreshTokenCommandHandler> logger)
    : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get stored refresh token
            var storedToken = await refreshTokenRepository.GetByTokenAsync(
                request.RefreshToken,
                cancellationToken);

            if (storedToken is null)
            {
                logger.LogWarning("Refresh token not found: {Token}", request.RefreshToken);
                return Error.NotFound("Auth.InvalidToken", "Invalid refresh token");
            }

            // Validate token status
            var validationResult = ValidateToken(storedToken);
            if (validationResult.IsFailure)
                return validationResult.Error!;

            // Get user
            var user = await userManager.FindByIdAsync(storedToken.UserId.ToString());
            if (user is null)
            {
                logger.LogError("User not found for refresh token: {UserId}", storedToken.UserId);
                return Error.NotFound("Auth.UserNotFound", "User not found");
            }

            // Get user roles
            var roles = await userManager.GetRolesAsync(user);

            // Generate new tokens
            var newAccessToken = jwtService.GenerateAccessToken(
                user.Id,
                user.Email!,
                user.UserName!,
                roles);

            var newRefreshToken = new Domain.Entities.RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = jwtService.GenerateRefreshToken(),
                JwtId = jwtService.GetJwtIdFromToken(newAccessToken),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                DeviceInfo = storedToken.DeviceInfo,
                IpAddress = storedToken.IpAddress,
                UserAgent = storedToken.UserAgent,
                CreatedAt = DateTime.UtcNow
            };

            // Mark old token as used and store replacement
            storedToken.IsUsed = true;
            storedToken.ReplacedByToken = newRefreshToken.Token;
            await refreshTokenRepository.UpdateAsync(storedToken, cancellationToken);

            // Save new token
            await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

            logger.LogInformation(
                "Refresh token successfully refreshed for user {UserId}",
                user.Id);

            return new RefreshTokenResponse(
                newAccessToken,
                newRefreshToken.Token,
                newRefreshToken.ExpiresAt,
                user.Id,
                user.Email!,
                roles.ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during token refresh");
            return Error.Failure("Auth.RefreshError", "An error occurred during token refresh");
        }
    }

    private static Result<bool> ValidateToken(Domain.Entities.RefreshToken token)
    {
        if (token.IsUsed)
            return Error.Validation("Auth.TokenUsed", "Refresh token has already been used");

        if (token.IsRevoked)
            return Error.Validation("Auth.TokenRevoked", "Refresh token has been revoked");

        if (token.IsExpired)
            return Error.Validation("Auth.TokenExpired", "Refresh token has expired");

        return true;
    }
}
