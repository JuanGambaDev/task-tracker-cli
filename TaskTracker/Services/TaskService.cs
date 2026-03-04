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

    public async Task<List<TaskItem>> GetTasksAsync(TaskItemStatus? status = null)
    {
        var tasks = await _repository.LoadAsync();

        return status is null
            ? tasks
            : tasks.Where(t => t.Status == status).ToList();
    }

    public async Task<TaskItem> UpdateTaskDescriptionAsync(int id, string description)
    {
        if (id <= 0)
            throw new ArgumentException("Task id must be greater than zero.");

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Task description cannot be empty.");

        var tasks = await _repository.LoadAsync();
        var task = tasks.FirstOrDefault(t => t.Id == id);

        if (task is null)
            throw new InvalidOperationException($"Task with id {id} not found.");

        task.Description = description.Trim();
        task.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveAsync(tasks);

        return task;
    }

    public async Task<TaskItem> UpdateStatusTaskAsync(string commandCli, int id)
    {
        if (id <= 0)
            throw new ArgumentException("Task id must be greater than zero.");

        if (string.IsNullOrWhiteSpace(commandCli))
            throw new ArgumentException("Command CLI cannot be empty.");

        var tasks = await _repository.LoadAsync();
        var task = tasks.FirstOrDefault(t => t.Id == id);

        if (task is null)
            throw new InvalidOperationException($"Task with id {id} not found.");

        // Guard against unknown status commands — prevents silent no-ops.
        task.Status = commandCli switch
        {
            "mark-in-progress" => TaskItemStatus.InProgress,
            "mark-in-done"     => TaskItemStatus.Done,
            _                  => throw new ArgumentException(
                                      $"Unknown status command '{commandCli}'. " +
                                      $"Use: mark-in-progress | mark-in-done")
        };

        task.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveAsync(tasks);

        return task;
    }

    public async Task<TaskItem> DeleteTaskByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Task id must be greater than zero.");

        var tasks = await _repository.LoadAsync();
        var task = tasks.FirstOrDefault(t => t.Id == id);

        if (task is null)
            throw new InvalidOperationException($"Task with id {id} not found.");

        tasks.Remove(task);
        await _repository.SaveAsync(tasks);

        return task;
    }
}
