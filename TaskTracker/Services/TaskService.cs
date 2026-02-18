using TaskTracker.Repositories;
using TaskTracker.Models;

namespace TaskTracker.Services;

public class TaskService
{
    private readonly IRepository<TaskItem> _repository;

    public TaskService(IRepository<TaskItem> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<TaskItem> AddTaskAsync(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Task description cannot be empty.");

        var tasks = await _repository.LoadAsync();

        var task = new TaskItem
        {
            Id = tasks.Any() ? tasks.Max(t => t.Id) + 1 : 1,
            Description = description.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        tasks.Add(task);
        await _repository.SaveAsync(tasks);

        return task;
    }

    public async Task<List<TaskItem>> GetAllTasksAsync(TaskItemStatus? status = null)
    {
        var tasks = await _repository.LoadAsync();

        return status is null
            ? tasks
            : tasks.Where(t => t.Status == status).ToList();
    }
}
