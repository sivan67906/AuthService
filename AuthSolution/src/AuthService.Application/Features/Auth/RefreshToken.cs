using AuthService.Application.Abstractions;
using AuthService.Contracts;
using AuthService.Domain.Common;
using AuthService.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AuthService.Application.Features.Auth;

public record RefreshTokenCommand(Guid UserId, string RefreshToken) : IRequest<Result<AuthDtos.RefreshTokenResponse>>;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<AuthDtos.RefreshTokenResponse>>
{
    private readonly IUserRepository _repo;
    private readonly ITokenService _tokenService;
    private readonly UserManager<AppUser> _userManager;

    public RefreshTokenHandler(IUserRepository repo, ITokenService tokenService, UserManager<AppUser> userManager)
    {
        _repo = repo;
        _tokenService = tokenService;
        _userManager = userManager;
    }

    public async Task<Result<AuthDtos.RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var userRes = await _repo.FindByEmailAsync((await _userManager.FindByIdAsync(request.UserId.ToString()))?.Email ?? "", ct);
        if (!userRes.IsSuccess) return Result<AuthDtos.RefreshTokenResponse>.Failure(userRes.Error);
        var user = userRes.Value!;

        var tokenRes = await _repo.GetRefreshTokenAsync(user.Id, request.RefreshToken, ct);
        if (!tokenRes.IsSuccess || tokenRes.Value!.Revoked || tokenRes.Value!.ExpiresAtUtc < DateTime.UtcNow)
            return Result<AuthDtos.RefreshTokenResponse>.Failure(new("auth.refresh_invalid","Invalid refresh token"));

        var roles = await _userManager.GetRolesAsync(user);
        var (access, exp) = _tokenService.CreateAccessToken(user, roles);
        var newRefresh = _tokenService.CreateRefreshToken();
        await _repo.AddRefreshTokenAsync(user.Id, new RefreshToken { Token = newRefresh, ExpiresAtUtc = DateTime.UtcNow.AddDays(14) }, ct);
        return Result<AuthDtos.RefreshTokenResponse>.Success(new(access, newRefresh, exp));
    }
}
