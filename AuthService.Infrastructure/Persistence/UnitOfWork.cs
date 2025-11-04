using AuthService.Domain.Common;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Persistence.Contexts;
using AuthService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace AuthService.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly CommandDbContext _context;
    private IDbContextTransaction? _transaction;
    private Dictionary<Type, object>? _repositories;

    public UnitOfWork(CommandDbContext context)
    {
        _context = context;
    }

    private IUserRepository? _users;
    private IAddressRepository? _addresses;
    private IRefreshTokenRepository? _refreshTokens;

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IAddressRepository Addresses => _addresses ??= new AddressRepository(_context);
    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_context);

    public IGenericRepository<T> Repository<T>() where T : BaseEntity
    {
        _repositories ??= new Dictionary<Type, object>();

        var type = typeof(T);
        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new GenericRepository<T>(_context);
        }

        return (IGenericRepository<T>)_repositories[type];
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            if (_transaction is not null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction is not null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
