namespace TaskTracker.Repositories;

public interface IRepository<T>
{
    Task<List<T>> LoadAsync();
    Task SaveAsync(List<T> items);
}
