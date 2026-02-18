using TaskTracker.Models;
using TaskTracker.Repositories;

namespace TaskTracker.Tests.Fakes;

public class FakeTaskRepository : IRepository<TaskItem>
{
    private readonly List<TaskItem> _tasks = new();

    public bool ThrowOnLoad { get; set; }
    public bool ThrowOnSave { get; set; }

    public Task<List<TaskItem>> LoadAsync()
    {
        if (ThrowOnLoad)
            throw new IOException("Simulated load failure");

        // Return a copy (important!)
        return Task.FromResult(_tasks.ToList());
    }

    public Task SaveAsync(List<TaskItem> items)
    {
        if (ThrowOnSave)
            throw new IOException("Simulated save failure");

        _tasks.Clear();
        _tasks.AddRange(items);

        return Task.CompletedTask;
    }

    // Helper for tests
    public IReadOnlyList<TaskItem> StoredTasks => _tasks;
}

