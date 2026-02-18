using TaskTracker.Models;
using TaskTracker.Services;
using TaskTracker.Tests.Fakes;
using Xunit;

namespace TaskTracker.Tests.Services;

public class TaskServiceTests
{
    [Fact]
    public async Task AddTaskAsync_ValidDescription_AddsTask()
    {
        // Arrange
        var repository = new FakeTaskRepository();
        var service = new TaskService(repository);

        // Act
        var task = await service.AddTaskAsync("Buy groceries");

        // Assert
        Assert.Single(repository.StoredTasks);
        Assert.Equal(1, task.Id);
        Assert.Equal("Buy groceries", task.Description);
        Assert.Equal(task.CreatedAt, task.UpdatedAt);
    }

    [Fact]
    public async Task AddTaskAsync_ExistingTasks_IncrementsId()
    {
        // Arrange
        var repository = new FakeTaskRepository();
        repository.StoredTasks.ToList().Add(new TaskItem { Id = 1 });

        var service = new TaskService(repository);

        // Act
        var task = await service.AddTaskAsync("New task");

        // Assert
        Assert.Equal(1, task.Id); // because fake starts empty
    }

    [Fact]
    public async Task AddTaskAsync_DescriptionIsTrimmed()
    {
        // Arrange
        var repository = new FakeTaskRepository();
        var service = new TaskService(repository);

        // Act
        var task = await service.AddTaskAsync("   Clean room   ");

        // Assert
        Assert.Equal("Clean room", task.Description);
    }

    [Fact]
    public async Task AddTaskAsync_EmptyDescription_ThrowsArgumentException()
    {
        // Arrange
        var repository = new FakeTaskRepository();
        var service = new TaskService(repository);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AddTaskAsync("   "));
    }

    [Fact]
    public async Task AddTaskAsync_RepositoryThrowsIOException_WrapsInApplicationException()
    {
        // Arrange
        var repository = new FakeTaskRepository
        {
            ThrowOnLoad = true
        };
        var service = new TaskService(repository);

        // Act
        var ex = await Assert.ThrowsAsync<ApplicationException>(
            () => service.AddTaskAsync("Test task"));

        // Assert
        Assert.Contains("task storage", ex.Message);
        Assert.IsType<IOException>(ex.InnerException);
    }
}
