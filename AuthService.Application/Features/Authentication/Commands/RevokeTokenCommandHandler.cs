using AuthService.Domain.Common;
using AuthService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Authentication.Commands;

public sealed class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RevokeTokenCommandHandler> _logger;

    public RevokeTokenCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<RevokeTokenCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken);

            if (token is null)
            {
                _logger.LogWarning("Revoke attempt failed: Token not found");
                return Result.Failure(new Error("Auth.TokenNotFound", "Refresh token not found"));
            }

            if (token.IsRevoked)
            {
                _logger.LogWarning("Token already revoked: {TokenId}", token.Id);
                return Result.Failure(new Error("Auth.AlreadyRevoked", "Token has already been revoked"));
            }

            await _unitOfWork.RefreshTokens.RevokeAsync(
                request.RefreshToken,
                request.RevokedByIp,
                cancellationToken
            );

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Refresh token revoked successfully: {TokenId}", token.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during token revocation");
            return Result.Failure(new Error("Auth.RevokeError", "An error occurred during token revocation"));
        }
    }
}
