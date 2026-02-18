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
            throw new ArgumentException("Task description cannot be empty.", nameof(description));

        try
        {
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
        catch (IOException ex)
        {
            throw new ApplicationException(
                "An error occurred while accessing the task storage.",
                ex);
        }
        catch (Exception ex)
        {
            throw new ApplicationException(
                "An unexpected error occurred while creating the task.",
                ex);
        }
    }
}
