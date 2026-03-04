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

    // ─── AddTaskAsync ──────────────────────────────────────────────────────────

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
    public async Task AddTaskAsync_TrimsDescription()
    {
        var service = CreateService();

        var task = await service.AddTaskAsync("  padded description  ");

        Assert.Equal("padded description", task.Description);
    }

    [Fact]
    public async Task AddTaskAsync_SetsDefaultStatusToDo()
    {
        var service = CreateService();

        var task = await service.AddTaskAsync("New task");

        Assert.Equal(TaskItemStatus.ToDo, task.Status);
    }

    // ─── GetTasksAsync ─────────────────────────────────────────────────────────

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
    public async Task GetTasksAsync_ReturnsEmptyList_WhenNoTasksExist()
    {
        var service = CreateService();

        var tasks = await service.GetTasksAsync();

        Assert.Empty(tasks);
    }

    // ─── UpdateTaskDescriptionAsync ────────────────────────────────────────────

    [Fact]
    public async Task UpdateTaskAsync_UpdatesTaskDescription()
    {
        var service = CreateService();

        var createdTask = await service.AddTaskAsync("Initial project");
        var updatedTask = await service.UpdateTaskDescriptionAsync(createdTask.Id, "update project");

        Assert.Equal(createdTask.Id, updatedTask.Id);
        Assert.Equal("update project", updatedTask.Description);
        Assert.True(updatedTask.UpdatedAt >= createdTask.UpdatedAt);
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
    public async Task UpdateTaskAsync_Throws_WhenIdIsNegative()
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateTaskDescriptionAsync(-5, "update project"));

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

    // ─── UpdateStatusAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStatusAsync_SetsStatusInProgress()
    {
        var service = CreateService();
        var task = await service.AddTaskAsync("Some task");

        var updated = await service.UpdateStatusAsync(task.Id, TaskItemStatus.InProgress);

        Assert.Equal(TaskItemStatus.InProgress, updated.Status);
        Assert.True(updated.UpdatedAt >= task.UpdatedAt);
    }

    [Fact]
    public async Task UpdateStatusAsync_SetsStatusDone()
    {
        var service = CreateService();
        var task = await service.AddTaskAsync("Some task");

        var updated = await service.UpdateStatusAsync(task.Id, TaskItemStatus.Done);

        Assert.Equal(TaskItemStatus.Done, updated.Status);
        Assert.True(updated.UpdatedAt >= task.UpdatedAt);
    }

    [Fact]
    public async Task UpdateStatusAsync_SetsStatusBackToToDo()
    {
        var service = CreateService();
        var task = await service.AddTaskAsync("Some task");

        await service.UpdateStatusAsync(task.Id, TaskItemStatus.Done);
        var reverted = await service.UpdateStatusAsync(task.Id, TaskItemStatus.ToDo);

        Assert.Equal(TaskItemStatus.ToDo, reverted.Status);
    }

    [Fact]
    public async Task UpdateStatusAsync_Throws_WhenIdIsZero()
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateStatusAsync(0, TaskItemStatus.Done));

        Assert.Contains("id", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateStatusAsync_Throws_WhenIdIsNegative()
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateStatusAsync(-1, TaskItemStatus.Done));

        Assert.Contains("id", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateStatusAsync_Throws_WhenTaskDoesNotExist()
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateStatusAsync(999, TaskItemStatus.Done));

        Assert.Contains("not found", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ─── DeleteTaskByIdAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task DeleteTaskById_DeletesTaskAndReturnsIt()
    {
        var service = CreateService();

        var task1 = await service.AddTaskAsync("test project");
        var task2 = await service.AddTaskAsync("add new task");

        var deletedTask = await service.DeleteTaskByIdAsync(task2.Id);

        Assert.Equal(task2.Id, deletedTask.Id);
        Assert.Equal("add new task", deletedTask.Description);

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
    public async Task DeleteTaskById_Throws_WhenIdIsNegative()
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.DeleteTaskByIdAsync(-1));

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
