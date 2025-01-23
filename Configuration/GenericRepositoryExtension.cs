using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Repository;

namespace Persistence.Configuration;

public static class GenericRepositoryExtension
{
    public static void RegisterGenericRepository<TContext, TEntity>(this IServiceCollection service)
    where TContext : DbContext
    where TEntity : class
    {
        service.AddScoped<IGenericRepository<TEntity>, GenericRepository<TContext, TEntity>>();
    }
}
