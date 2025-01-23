using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Persistence.UnitOfWork;

public class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    private readonly TContext _context;
    private IDbContextTransaction _transaction;
    private bool _disposed = false;
    private Dictionary<Type, object> _repositories = new Dictionary<Type, object>();

    public UnitOfWork(TContext context)
    {
        _context = context;
    }

    public TRepository? Repository<TRepository>()
        where TRepository : class
    {
        if (_repositories.ContainsKey(typeof(TRepository)))
        {
            return _repositories[typeof(TRepository)] as TRepository;
        }

        return null;
    }

    public void AddRepository<TRepository>(TRepository repository) where TRepository : class
    {
        if (!_repositories.ContainsKey(typeof(TRepository)))
        {
            _repositories.Add(typeof(TRepository), repository!);
        }
    }

    public async Task BeginTransactionAsync() => _transaction = await _context.Database.BeginTransactionAsync();

    public async Task RollbackTransactionAsync()
    {
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _transaction = null!;
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _transaction.CommitAsync();
        }
        catch
        {
            await _transaction.RollbackAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null!;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing) _context.Dispose();
        }

        _disposed = true;
    }
}
