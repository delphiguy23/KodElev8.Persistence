# KodElev8.Persistence

A robust .NET 8.0 persistence layer library that provides a generic repository pattern implementation with Entity Framework Core and PostgreSQL support.

## Project Structure

```
KodElev8.Persistence/
├── Configuration/       # Database and other configuration settings
├── CrudResult/         # CRUD operation result types
├── Models/             # Data models and entities
├── Repository/         # Generic repository implementation
├── UnitOfWork/        # Unit of Work pattern implementation
├── Extensions/        # Extension methods
└── Initializable/     # Initialization-related code
```

## Technology Stack

- .NET 8.0
- Entity Framework Core 8.0.7
- PostgreSQL (Npgsql.EntityFrameworkCore.PostgreSQL 8.0.4)
- MassTransit 8.3.1 (for message-based architecture)
- Serilog with Seq integration (for logging)
- Microsoft.AspNetCore.JsonPatch (for PATCH operations)

## Features

- Generic Repository Pattern implementation
- Unit of Work Pattern for transaction management
- CRUD operations with strongly-typed results
- Support for JSON Patch operations
- Async/await first approach
- Flexible querying with expressions
- Bulk operations support
- Soft delete capability

## Using the Generic Repository

### 1. Define Your Entity

```csharp
public class YourEntity : IEntityModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    // ... other properties
}
```

### 2. Create Your DbContext

```csharp
public class YourDbContext : DbContext
{
    public YourDbContext(DbContextOptions<YourDbContext> options) : base(options)
    {
    }

    public DbSet<YourEntity> YourEntities { get; set; }
}
```

### 3. Implement the Repository

```csharp
public class YourEntityRepository : GenericRepository<YourDbContext, YourEntity>
{
    public YourEntityRepository(YourDbContext context, ILogger<GenericRepository<YourDbContext, YourEntity>> logger)
        : base(context, logger)
    {
    }
}
```

### 4. Usage Examples

```csharp
// Inject the repository
private readonly IGenericRepository<YourEntity> _repository;

// Create
var entity = new YourEntity { Name = "Test" };
var result = await _repository.Add(entity);

// Read
var item = await _repository.GetById(id);
var all = await _repository.GetAll();

// Query with expression
var filtered = await _repository.GetBy(x => x.Name == "Test");

// Update
entity.Name = "Updated";
var updated = await _repository.Update(entity);

// Delete
var deleted = await _repository.Delete(id);

// Patch
var patchDoc = new JsonPatchDocument<YourEntity>();
patchDoc.Replace(x => x.Name, "New Name");
var patched = await _repository.Patch(id, patchDoc);

// Upsert
var upserted = await _repository.Upsert(entity);

// Ordered query with limit
var ordered = await _repository.GetBy(
    x => x.Name.Contains("Test"),
    x => x.Name,
    ascending: true,
    limit: 10
);
```

## Contributing

Please read our contributing guidelines and code of conduct before submitting pull requests.

## License

This project is licensed under the terms of the license included in the repository.
