using System.Linq.Expressions;
using AuthService.Domain.Common;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Base repository implementation with direct DbContext access (NO UnitOfWork)
/// </summary>
public class Repository<T>(
    CommandDbContext context,
    ILogger<Repository<T>> logger) : IRepository<T> where T : BaseEntity
{
    protected readonly CommandDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();
    protected readonly ILogger<Repository<T>> Logger = logger;

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await DbSet.FindAsync([id], cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting entity by ID: {Id}", id);
            throw;
        }
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await DbSet.ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting all entities");
            throw;
        }
    }

    public virtual async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await DbSet
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error finding entities with predicate");
            throw;
        }
    }

    public virtual async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting first entity with predicate");
            throw;
        }
    }

    public virtual async Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await DbSet.AnyAsync(predicate, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking if any entity matches predicate");
            throw;
        }
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return predicate is null
                ? await DbSet.CountAsync(cancellationToken)
                : await DbSet.CountAsync(predicate, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error counting entities");
            throw;
        }
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await DbSet.AddAsync(entity, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            
            Logger.LogInformation("Entity added: {EntityType} with ID: {Id}", 
                typeof(T).Name, entity.Id);
            
            return entity;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error adding entity: {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await DbSet.AddRangeAsync(entities, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            
            Logger.LogInformation("Entities added: {EntityType}, Count: {Count}", 
                typeof(T).Name, entities.Count());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error adding entities: {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            entity.UpdatedAt = DateTime.UtcNow;
            DbSet.Update(entity);
            await SaveChangesAsync(cancellationToken);
            
            Logger.LogInformation("Entity updated: {EntityType} with ID: {Id}", 
                typeof(T).Name, entity.Id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating entity: {EntityType} with ID: {Id}", 
                typeof(T).Name, entity.Id);
            throw;
        }
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            DbSet.Remove(entity);
            await SaveChangesAsync(cancellationToken);
            
            Logger.LogInformation("Entity deleted: {EntityType} with ID: {Id}", 
                typeof(T).Name, entity.Id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting entity: {EntityType} with ID: {Id}", 
                typeof(T).Name, entity.Id);
            throw;
        }
    }

    public virtual async Task DeleteRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default)
    {
        try
        {
            DbSet.RemoveRange(entities);
            await SaveChangesAsync(cancellationToken);
            
            Logger.LogInformation("Entities deleted: {EntityType}, Count: {Count}", 
                typeof(T).Name, entities.Count());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting entities: {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await Context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, "Database update error");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving changes");
            throw;
        }
    }
}
