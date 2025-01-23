namespace Persistence.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    TRepository? Repository<TRepository>()
        where TRepository : class;

    void AddRepository<TRepository>(TRepository repository)
        where TRepository : class;

    Task BeginTransactionAsync();
    Task RollbackTransactionAsync();
    Task CommitTransactionAsync();
}
