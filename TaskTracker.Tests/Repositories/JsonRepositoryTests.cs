using TaskTracker.Repositories;
using Xunit;

namespace TaskTracker.Tests;

public class JsonRepositoryTests : TestBase
{
    [Fact]
    public async Task LoadAsync_ReturnsEmptyList_WhenFileDoesNotExist()
    {
        var repo = new JsonRepository<string>(TempFilePath);

        var result = await repo.LoadAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SaveAndLoadAsync_PersistsDataCorrectly()
    {
        var repo = new JsonRepository<int>(TempFilePath);

        var data = new List<int> { 1, 2, 3 };

        await repo.SaveAsync(data);
        var loaded = await repo.LoadAsync();

        Assert.Equal(data, loaded);
    }
}