using System.Linq.Expressions;
using Microsoft.AspNetCore.JsonPatch;
using Persistence.CrudResult;
using Results;

namespace Persistence.Repository;

public interface IGenericRepository<TEntity>
    where TEntity : class
{
    Task<IResults<IEnumerable<TEntity>>> GetAll(CancellationToken cancellationToken = default);
    Task<IResults<TEntity>> GetById(object id, CancellationToken cancellationToken = default);
    Task<IResults<CrudResult<TEntity>>> Add(TEntity obj, CancellationToken cancellationToken = default);

    Task<IResults<CrudResult<IEnumerable<TEntity>>>> Add(IEnumerable<TEntity> obj,
        CancellationToken cancellationToken = default);

    Task<IResults<CrudResult<TEntity>>> Update(TEntity obj, CancellationToken cancellationToken = default);
    Task<IResults<CrudResult<TEntity>>> Delete(object id, CancellationToken cancellationToken = default);

    Task<IResults<CrudResult<TEntity>>> Delete(object id, string activeEntityField,
        CancellationToken cancellationToken = default);

    Task<IResults<CrudResult<TEntity>>> Patch(object id, JsonPatchDocument<TEntity> patch,
        CancellationToken cancellationToken = default);

    Task<IResults<IEnumerable<TEntity>>> GetBy(Expression<Func<TEntity, bool>> predicate,
        int limit = 0, CancellationToken cancellationToken = default);

    Task<IResults<IEnumerable<TEntity>>> GetBy(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, object>> orderBy,
        bool ascending = true,
        int limit = 0, CancellationToken cancellationToken = default);

    Task<IResults<CrudResult<IEnumerable<TEntity>>>> Upsert(IEnumerable<TEntity> obj,
        CancellationToken cancellationToken = default);

    Task<IResults<CrudResult<TEntity>>> Upsert(TEntity obj,
        CancellationToken cancellationToken = default);

    Task<IResults<TEntity>> FirstOrDefault(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<IResults<CrudResult<TEntity>>> UpdateFields(TEntity obj, string[] fields,
        CancellationToken cancellationToken = default);

    Task<IResults<IEnumerable<TEntity>>> TakeBy(Expression<Func<TEntity, bool>> predicate,
        int limit, CancellationToken cancellationToken = default);

}
