namespace Persistence.Models;

public interface IEntityLoginModel
{
    Guid Id { get; }
    DateTime LoggedOn { get; }
}
