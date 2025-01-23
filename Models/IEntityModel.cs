namespace Persistence.Models;

public interface IEntityModel
{
    Guid Id { get; }
    DateTime CreatedOn { get; }
    string CreatedBy { get; }
    bool Active { get; }
}
