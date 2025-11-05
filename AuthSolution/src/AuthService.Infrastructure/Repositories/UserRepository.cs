using AuthService.Application.Abstractions;
using AuthService.Domain.Common;
using AuthService.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly WriteDbContext _write;
    private readonly UserManager<AppUser> _userManager;

    public UserRepository(WriteDbContext write, UserManager<AppUser> userManager)
    {
        _write = write;
        _userManager = userManager;
    }

    public async Task<Result<AppUser>> CreateUserAsync(AppUser user, string password, CancellationToken ct)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return Result<AppUser>.Failure(new Error("identity.create_failed", string.Join(",", result.Errors.Select(e => e.Description))));
        return Result<AppUser>.Success(user);
    }

    public async Task<Result<AppUser>> FindByEmailAsync(string email, CancellationToken ct)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
        return user is null
            ? Result<AppUser>.Failure(new Error("user.not_found", "User not found"))
            : Result<AppUser>.Success(user);
    }

    public async Task<Result> AddAddressAsync(Guid userId, Address address, CancellationToken ct)
    {
        address.UserId = userId;
        await _write.Addresses.AddAsync(address, ct);
        await _write.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<Address>>> GetAddressesAsync(Guid userId, CancellationToken ct)
    {
        var addrs = await _write.Addresses.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAtUtc).ToListAsync(ct);
        return Result<IReadOnlyList<Address>>.Success(addrs);
    }

    public async Task<Result> AddRefreshTokenAsync(Guid userId, RefreshToken token, CancellationToken ct)
    {
        token.UserId = userId;
        await _write.RefreshTokens.AddAsync(token, ct);
        await _write.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<RefreshToken>> GetRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken ct)
    {
        var token = await _write.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId && x.Token == refreshToken, ct);
        return token is null ? Result<RefreshToken>.Failure(new Error("token.not_found","Refresh token not found")) : Result<RefreshToken>.Success(token);
    }

    public async Task<Result> RevokeRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken ct)
    {
        var token = await _write.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId && x.Token == refreshToken, ct);
        if (token is null) return Result.Failure(new Error("token.not_found", "Refresh token not found"));
        token.Revoked = true;
        await _write.SaveChangesAsync(ct);
        return Result.Success();
    }
}
