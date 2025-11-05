using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(
    CommandDbContext context,
    ILogger<UserRepository> logger) : Repository<ApplicationUser>(context, logger), IUserRepository
{
    public async Task<ApplicationUser?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await DbSet
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting user by email: {Email}", email);
            throw;
        }
    }

    public async Task<ApplicationUser?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await DbSet
                .FirstOrDefaultAsync(u => u.UserName == username, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting user by username: {Username}", username);
            throw;
        }
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await DbSet.AnyAsync(u => u.Email == email, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking if email exists: {Email}", email);
            throw;
        }
    }

    public async Task<bool> UsernameExistsAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await DbSet.AnyAsync(u => u.UserName == username, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking if username exists: {Username}", username);
            throw;
        }
    }

    public async Task<ApplicationUser?> GetWithAddressesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await DbSet
                .Include(u => u.Addresses.Where(a => !a.IsDeleted))
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting user with addresses: {UserId}", userId);
            throw;
        }
    }

    public async Task<ApplicationUser?> GetWithRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Note: For roles, we rely on UserManager which handles role loading
            // This method returns the user; roles are loaded separately via UserManager
            return await GetByIdAsync(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting user with roles: {UserId}", userId);
            throw;
        }
    }
}
