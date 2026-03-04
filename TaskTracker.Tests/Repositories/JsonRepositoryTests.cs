using System.Text.Json;
using TaskTracker.Models;
using TaskTracker.Repositories;
using Xunit;

namespace TaskTracker.Tests;

public class JsonRepositoryTests : TestBase
{
    // ─── LoadAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadAsync_ReturnsEmptyList_WhenFileDoesNotExist()
    {
        var repo = new JsonRepository<string>(TempFilePath);

        var result = await repo.LoadAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task LoadAsync_ReturnsEmptyList_WhenFileIsEmpty()
    {
        await File.WriteAllTextAsync(TempFilePath, string.Empty);

        var repo = new JsonRepository<string>(TempFilePath);

        await Assert.ThrowsAsync<IOException>(() => repo.LoadAsync());
    }

    [Fact]
    public async Task LoadAsync_ThrowsIOException_WhenFileContainsCorruptedJson()
    {
        await File.WriteAllTextAsync(TempFilePath, "{ this is not valid json !!!");

        var repo = new JsonRepository<string>(TempFilePath);

        var ex = await Assert.ThrowsAsync<IOException>(() => repo.LoadAsync());

        Assert.Contains("invalid JSON", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(TempFilePath, ex.Message);
    }

    [Fact]
    public async Task LoadAsync_ThrowsIOException_WhenFileContainsWrongJsonType()
    {
        await File.WriteAllTextAsync(TempFilePath, "{ \"key\": 42 }");

        var repo = new JsonRepository<string>(TempFilePath);

        await Assert.ThrowsAsync<IOException>(() => repo.LoadAsync());
    }

    // ─── SaveAsync + LoadAsync round-trip ──────────────────────────────────────

    [Fact]
    public async Task SaveAndLoadAsync_PersistsDataCorrectly()
    {
        var repo = new JsonRepository<int>(TempFilePath);
        var data = new List<int> { 1, 2, 3 };

        await repo.SaveAsync(data);
        var loaded = await repo.LoadAsync();

        Assert.Equal(data, loaded);
    }

    [Fact]
    public async Task SaveAsync_OverwritesPreviousData()
    {
        var repo = new JsonRepository<int>(TempFilePath);

        await repo.SaveAsync(new List<int> { 1, 2, 3 });
        await repo.SaveAsync(new List<int> { 99 });

        var loaded = await repo.LoadAsync();

        Assert.Single(loaded);
        Assert.Equal(99, loaded[0]);
    }

    [Fact]
    public async Task SaveAndLoadAsync_PreservesEmptyList()
    {
        var repo = new JsonRepository<string>(TempFilePath);

        await repo.SaveAsync(new List<string>());
        var loaded = await repo.LoadAsync();

        Assert.NotNull(loaded);
        Assert.Empty(loaded);
    }

    // ─── Enum serialisation ────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_SerializesTaskItemStatusAsString()
    {
        var repo = new JsonRepository<TaskItem>(TempFilePath);

        var task = new TaskItem
        {
            Id = 1,
            Description = "Test task",
            Status = TaskItemStatus.InProgress,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await repo.SaveAsync(new List<TaskItem> { task });

        var raw = await File.ReadAllTextAsync(TempFilePath);

        // Status must appear as a human-readable string, not a raw integer.
        Assert.Contains("InProgress", raw);
        Assert.DoesNotContain("\"Status\": 1", raw);
    }

    [Fact]
    public async Task SaveAndLoadAsync_RoundTrips_EnumValuesCorrectly()
    {
        var repo = new JsonRepository<TaskItem>(TempFilePath);

        var task = new TaskItem
        {
            Id = 1,
            Description = "Test task",
            Status = TaskItemStatus.Done,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await repo.SaveAsync(new List<TaskItem> { task });
        var loaded = await repo.LoadAsync();

        Assert.Single(loaded);
        Assert.Equal(TaskItemStatus.Done, loaded[0].Status);
    }
}
