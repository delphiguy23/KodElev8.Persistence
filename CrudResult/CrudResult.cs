namespace Persistence.CrudResult;

public class CrudResult<T> : ICrudResult<T> where T : class
{
    public int Count { get; set; }
    public T? Entity { get; set; }
}

public class CrudResult
{
    public int Count { get; set; }
}
