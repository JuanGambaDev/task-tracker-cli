using System.Diagnostics.CodeAnalysis;
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

        // Manually update status (since your service doesn’t yet)
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
        // Arrange
        var service = CreateService();

        var createdTask = await service.AddTaskAsync("Initial project");

        // Act
        var updatedTask = await service.UpdateTaskDescriptionAsync(
            createdTask.Id,
            "update project");

        // Assert
        Assert.Equal(createdTask.Id, updatedTask.Id);
        Assert.Equal("update project", updatedTask.Description);
        Assert.True(updatedTask.UpdatedAt > createdTask.UpdatedAt);
    }

    [Fact]
    public async Task UpdateTaskAsync_Throws_WhenIdIsZero()
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateTaskDescriptionAsync(0, "update project"));

        Assert.Contains("id", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateTaskAsync_Throws_WhenDescriptionIsEmpty()
    {
        // Arrange
        var service = CreateService();
        var task = await service.AddTaskAsync("Initial project");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateTaskDescriptionAsync(task.Id, "   "));

        Assert.Contains("description", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateTaskAsync_Throws_WhenTaskDoesNotExist()
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateTaskDescriptionAsync(999, "update project"));

        Assert.Contains("not found", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}