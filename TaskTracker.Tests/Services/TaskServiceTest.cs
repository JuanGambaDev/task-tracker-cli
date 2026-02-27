using TaskTracker.Models;
using TaskTracker.Repositories;
using TaskTracker.Services;
using Xunit;

namespace TaskTracker.Tests;

public class TaskServiceTests : TestBase
{
    private TaskService CreateService()
    {
        var repository = new JsonRepository<TaskItem>(TempFilePath);
        return new TaskService(repository);
    }

    [Fact]
    public async Task AddTaskAsync_CreatesTaskWithIncrementedId()
    {
        var service = CreateService();

        var task1 = await service.AddTaskAsync("First task");
        var task2 = await service.AddTaskAsync("Second task");

        Assert.Equal(1, task1.Id);
        Assert.Equal(2, task2.Id);
    }

    [Fact]
    public async Task AddTaskAsync_ThrowsWhenDescriptionIsEmpty()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AddTaskAsync(" "));
    }

    [Fact]
    public async Task GetTasksAsync_ReturnsAllTasks()
    {
        var service = CreateService();

        await service.AddTaskAsync("Task A");
        await service.AddTaskAsync("Task B");

        var tasks = await service.GetTasksAsync();

        Assert.Equal(2, tasks.Count);
    }

    [Fact]
    public async Task GetTasksAsync_FiltersByStatus()
    {
        var service = CreateService();

        var task1 = await service.AddTaskAsync("ToDo task");
        var task2 = await service.AddTaskAsync("Done task");

        task2.Status = TaskItemStatus.Done;

        var repo = new JsonRepository<TaskItem>(TempFilePath);
        await repo.SaveAsync(new() { task1, task2 });

        var doneTasks = await service.GetTasksAsync(TaskItemStatus.Done);

        Assert.Single(doneTasks);
        Assert.Equal(TaskItemStatus.Done, doneTasks[0].Status);
    }

    [Fact]
    public async Task UpdateTaskAsync_UpdatesTaskDescription()
    {
        var service = CreateService();

        var createdTask = await service.AddTaskAsync("Initial project");

        var updatedTask = await service.UpdateTaskDescriptionAsync(
            createdTask.Id,
            "update project");

        Assert.Equal(createdTask.Id, updatedTask.Id);
        Assert.Equal("update project", updatedTask.Description);
        Assert.True(updatedTask.UpdatedAt > createdTask.UpdatedAt);
    }

    [Fact]
    public async Task UpdateTaskAsync_Throws_WhenIdIsZero()
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateTaskDescriptionAsync(0, "update project"));

        Assert.Contains("id", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateTaskAsync_Throws_WhenDescriptionIsEmpty()
    {
        var service = CreateService();
        var task = await service.AddTaskAsync("Initial project");

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateTaskDescriptionAsync(task.Id, "   "));

        Assert.Contains("description", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateTaskAsync_Throws_WhenTaskDoesNotExist()
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateTaskDescriptionAsync(999, "update project"));

        Assert.Contains("not found", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ─── UpdateStatusTaskAsync tests ───────────────────────────────────────────

    [Fact]
    public async Task UpdateStatusTaskAsync_SetsStatusInProgress()
    {
        var service = CreateService();
        var task = await service.AddTaskAsync("Some task");

        var updated = await service.UpdateStatusTaskAsync("mark-in-progress", task.Id);

        Assert.Equal(TaskItemStatus.InProgress, updated.Status);
        Assert.True(updated.UpdatedAt >= task.UpdatedAt);
    }

    [Fact]
    public async Task UpdateStatusTaskAsync_SetsStatusDone()
    {
        var service = CreateService();
        var task = await service.AddTaskAsync("Some task");

        var updated = await service.UpdateStatusTaskAsync("mark-in-done", task.Id);

        Assert.Equal(TaskItemStatus.Done, updated.Status);
        Assert.True(updated.UpdatedAt >= task.UpdatedAt);
    }

    [Fact]
    public async Task UpdateStatusTaskAsync_Throws_WhenIdIsZero()
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateStatusTaskAsync("mark-in-done", 0));

        Assert.Contains("id", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateStatusTaskAsync_Throws_WhenCommandIsEmpty()
    {
        var service = CreateService();
        var task = await service.AddTaskAsync("Some task");

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateStatusTaskAsync("   ", task.Id));

        Assert.Contains("command", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateStatusTaskAsync_Throws_WhenTaskDoesNotExist()
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateStatusTaskAsync("mark-in-done", 999));

        Assert.Contains("not found", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteTaskById_DeletesTaskAndReturnsIt()
    {
        var service = CreateService();

        var task1 = await service.AddTaskAsync("test project");
        var task2 = await service.AddTaskAsync("add new task");

        var deletedTask = await service.DeleteTaskByIdAsync(task2.Id);

        // Assert returned task is correct
        Assert.Equal(task2.Id, deletedTask.Id);
        Assert.Equal("add new task", deletedTask.Description);

        // Assert task was actually removed
        var remainingTasks = await service.GetTasksAsync();
        Assert.Single(remainingTasks);
        Assert.Equal(task1.Id, remainingTasks[0].Id);
    }

        [Fact]
    public async Task DeleteTaskById_Throws_WhenIdIsZero()
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.DeleteTaskByIdAsync(0));

        Assert.Contains("id", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteTaskById_Throws_WhenTaskDoesNotExist()
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DeleteTaskByIdAsync(999));

        Assert.Contains("not found", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}