using System.Linq.Expressions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.CrudResult;
using Persistence.Models;
using Results;
using Results.Extension;
using ILogger = Serilog.ILogger;

namespace Persistence.Repository;

public class GenericRepository<TContext, TEntity> : IGenericRepository<TEntity>
    where TContext : DbContext
    where TEntity : class
{
    public ILogger<GenericRepository<TContext, TEntity>> Logger { get; }
    private readonly TContext _context;
    private readonly ILogger _logger;
    private readonly DbSet<TEntity> _entity;

    protected GenericRepository(TContext context, ILogger<GenericRepository<TContext, TEntity>> logger)
    {
        Logger = logger;
        _context = context;
        _entity = context.Set<TEntity>();
    }

    public virtual async Task<IResults<TEntity>> GetById(object id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _entity.FindAsync(id, cancellationToken);

            return result == null ? ResultsTo.NotFound<TEntity>() : ResultsTo.Something(result);
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public virtual async Task<IResults<IEnumerable<TEntity>>> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await _entity.AsNoTracking().ToListAsync(cancellationToken);
        return ResultsTo.Something<IEnumerable<TEntity>>(result);
    }

    public virtual async Task<IResults<CrudResult<TEntity>>> Update(TEntity obj,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _entity.Update(obj);
            var result = await _context.SaveChangesAsync(cancellationToken);
            return ResultsTo.Updated(new CrudResult<TEntity> { Count = result, Entity = obj });
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public virtual async Task<IResults<CrudResult<TEntity>>> Add(TEntity obj,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _entity.AddAsync(obj, cancellationToken);
            var count = await _context.SaveChangesAsync(cancellationToken);

            return ResultsTo.Created(new CrudResult<TEntity> { Count = count, Entity = result.Entity });
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public virtual async Task<IResults<CrudResult<IEnumerable<TEntity>>>> Add(IEnumerable<TEntity> obj,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _entity.AddRangeAsync(obj, cancellationToken);
            var result = await _context.SaveChangesAsync(cancellationToken);
            //var ids = obj.Select(e => _context.Entry(e).Property("Id").CurrentValue).ToList();
            return ResultsTo.Created(new CrudResult<IEnumerable<TEntity>> { Count = result, Entity = obj });
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public virtual async Task<IResults<CrudResult<TEntity>>> Delete(object id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var forDelete = await GetById(id, cancellationToken);

            if (forDelete.IsNotFoundOrBadRequest())
            {
                return ResultsTo.NotFound(new CrudResult<TEntity> { Count = 0, Entity = null });
            }

            _entity.Remove(forDelete.Value!);
            var result = await _context.SaveChangesAsync(cancellationToken);
            return ResultsTo.Deleted(new CrudResult<TEntity> { Count = result, Entity = forDelete.Value! });
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public virtual async Task<IResults<CrudResult<TEntity>>> Delete(object id, string activeEntityField,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var forDelete = await GetById(id, cancellationToken);

            if (forDelete.IsNotFoundOrBadRequest())
            {
                return ResultsTo.NotFound(new CrudResult<TEntity> { Count = 0, Entity = null });
            }

            forDelete.Value!.GetType().GetProperty(activeEntityField)?.SetValue(forDelete.Value, false);
            var result = await _context.SaveChangesAsync(cancellationToken);
            return ResultsTo.Deleted(new CrudResult<TEntity> { Count = result, Entity = forDelete.Value! });
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public virtual async Task<IResults<CrudResult<TEntity>>> Patch(object id, JsonPatchDocument<TEntity> patch,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var forPatching = await GetById(id, cancellationToken);

            if (forPatching.IsNotFoundOrBadRequest())
            {
                return ResultsTo.NotFound(new CrudResult<TEntity> { Count = 0, Entity = null });
            }

            patch.ApplyTo(forPatching.Value);
            var result = await _context.SaveChangesAsync(cancellationToken);

            return ResultsTo.Updated(new CrudResult<TEntity> { Count = result, Entity = forPatching.Value! });
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async Task<IResults<CrudResult<IEnumerable<TEntity>>>> Upsert(IEnumerable<TEntity> obj,
        CancellationToken cancellationToken = default)
    {
        try
        {

            var entities = new List<TEntity>();

            foreach (var o in obj)
            {
                if (o is not IEntityModel)
                    return ResultsTo.Failure<CrudResult<IEnumerable<TEntity>>>()
                        .WithMessage("Entity does not implement IEntityModel");

                var entity = o as IEntityModel;

                if (entity is null)
                    return ResultsTo.Failure<CrudResult<IEnumerable<TEntity>>>()
                        .WithMessage("Entity does not implement IEntityModel");

                if (entity.Id == Guid.Empty)
                {
                    var result = await _entity.AddAsync(o, cancellationToken);
                    entities.Add(result.Entity);
                }
                else
                {
                    var result = _entity.Update(o);
                    entities.Add(result.Entity);
                }
            }

            var count = await _context.SaveChangesAsync(cancellationToken);
            return ResultsTo.Success(new CrudResult<IEnumerable<TEntity>> { Count = count, Entity = entities });
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async Task<IResults<CrudResult<TEntity>>> Upsert(TEntity obj, CancellationToken cancellationToken = default)
    {
        try
        {
                if (obj is not IEntityModel)
                    return ResultsTo.Failure<CrudResult<TEntity>>()
                        .WithMessage("Entity does not implement IEntityModel");

                var entity = obj as IEntityModel;

                if (entity is null)
                    return ResultsTo.Failure<CrudResult<TEntity>>()
                        .WithMessage("Entity does not implement IEntityModel");

                if (entity.Id == Guid.Empty)
                {
                    var resultAdd = await _entity.AddAsync(obj, cancellationToken);
                    return ResultsTo.Success(new CrudResult<TEntity> { Count = await _context.SaveChangesAsync(cancellationToken), Entity = resultAdd.Entity });
                }

                var resultUpdate = _entity.Update(obj);
                return ResultsTo.Success(new CrudResult<TEntity> { Count = await _context.SaveChangesAsync(cancellationToken), Entity = resultUpdate.Entity });
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async Task<IResults<TEntity>> FirstOrDefault(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _entity.FirstOrDefaultAsync(predicate, cancellationToken);

            return result == null ? ResultsTo.NotFound<TEntity>() : ResultsTo.Something(result);
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public virtual async Task<IResults<IEnumerable<TEntity>>> GetBy(Expression<Func<TEntity, bool>> predicate,
        int limit = 0, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _entity
                .AsNoTracking()
                .Where(predicate);

            if (limit > 0)
            {
                query = query.Take(limit);
            }

            var result = await query.ToListAsync(cancellationToken);

            return ResultsTo.Something<IEnumerable<TEntity>>(result);
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public virtual async Task<IResults<IEnumerable<TEntity>>> GetBy(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, object>> orderBy,
        bool ascending = true,
        int limit = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _entity
                .AsNoTracking()
                .Where(predicate);

            if (orderBy != null)
            {
                query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            }

            if (limit > 0)
            {
                query = query.Take(limit);
            }

            var result = await query.ToListAsync(cancellationToken);

            return ResultsTo.Something<IEnumerable<TEntity>>(result);
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async Task<IResults<CrudResult<TEntity>>> UpdateFields(TEntity obj, string[] fields, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await GetById(obj.GetType().GetProperty("Id")?.GetValue(obj), cancellationToken);

            if (entity.IsNotFoundOrBadRequest())
            {
                return ResultsTo.NotFound(new CrudResult<TEntity> { Count = 0, Entity = null });
            }

            foreach (var field in fields)
            {
                var property = entity.Value!.GetType().GetProperty(field);
                var value = obj.GetType().GetProperty(field)?.GetValue(obj);
                property?.SetValue(entity.Value, value);
            }

            var result = await _context.SaveChangesAsync(cancellationToken);

            return ResultsTo.Updated(new CrudResult<TEntity> { Count = result, Entity = entity.Value! });
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public virtual async Task<IResults<IEnumerable<TEntity>>> TakeBy(
        Expression<Func<TEntity, bool>> predicate,
        int limit,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _entity
                .AsNoTracking()
                .Where(predicate)
                .Take(limit);

            var result = await query.ToListAsync(cancellationToken);

            return ResultsTo.Something<IEnumerable<TEntity>>(result);
        }
        catch (Exception e)
        {
            throw;
        }
    }
}
