using TaskTracker.Repositories;
using Xunit;
using System.Text.Json;
using TaskTracker.Tests.Models;

namespace TaskTracker.Tests.Repositories;

public class JsonRepositoryTests
{
    private static string CreateTempFilePath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.json"
        );
    }

    [Fact]
    public async Task LoadAsync_FileDoesNotExist_ReturnsEmptyList()
    {
        // Arrange
        var filePath = CreateTempFilePath();
        var repository = new JsonRepository<TestItem>(filePath);

        // Act
        var result = await repository.LoadAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SaveAsync_ThenLoadAsync_ReturnsSavedItems()
    {
        // Arrange
        var filePath = CreateTempFilePath();
        var repository = new JsonRepository<TestItem>(filePath);

        var items = new List<TestItem>
        {
            new() { Id = 1, Name = "Test 1" },
            new() { Id = 2, Name = "Test 2" }
        };

        // Act
        await repository.SaveAsync(items);
        var result = await repository.LoadAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Test 1", result[0].Name);
        Assert.Equal("Test 2", result[1].Name);
    }

    [Fact]
    public async Task LoadAsync_InvalidJson_ThrowsIOException()
    {
        // Arrange
        var filePath = CreateTempFilePath();
        await File.WriteAllTextAsync(filePath, "INVALID JSON");

        var repository = new JsonRepository<TestItem>(filePath);

        // Act & Assert
        await Assert.ThrowsAsync<IOException>(() => repository.LoadAsync());
    }
}
