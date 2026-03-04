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
        // An empty file is not valid JSON — repo should surface IOException.
        // Some implementations tolerate it; ensure it never returns null.
        await File.WriteAllTextAsync(TempFilePath, string.Empty);

        var repo = new JsonRepository<string>(TempFilePath);

        // Empty file produces a JsonException internally → wrapped as IOException.
        await Assert.ThrowsAsync<IOException>(() => repo.LoadAsync());
    }

    [Fact]
    public async Task LoadAsync_ThrowsIOException_WhenFileContainsCorruptedJson()
    {
        // Acceptance criterion: corrupted JSON must never crash the app —
        // the repository wraps JsonException in a clear IOException.
        await File.WriteAllTextAsync(TempFilePath, "{ this is not valid json !!!");

        var repo = new JsonRepository<string>(TempFilePath);

        var ex = await Assert.ThrowsAsync<IOException>(() => repo.LoadAsync());

        // Message must be actionable, not a raw serialiser dump.
        Assert.Contains("invalid JSON", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(TempFilePath, ex.Message);
    }

    [Fact]
    public async Task LoadAsync_ThrowsIOException_WhenFileContainsWrongJsonType()
    {
        // Valid JSON but wrong shape (object instead of array).
        await File.WriteAllTextAsync(TempFilePath, "{ \"key\": 42 }");

        var repo = new JsonRepository<string>(TempFilePath);

        // Deserialising an object into List<string> throws JsonException → IOException.
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

    [Fact]
    public async Task SaveAsync_WritesValidJsonToFile()
    {
        var repo = new JsonRepository<int>(TempFilePath);

        await repo.SaveAsync(new List<int> { 7, 8, 9 });

        var raw = await File.ReadAllTextAsync(TempFilePath);

        // File must contain recognisable JSON — not binary or empty.
        Assert.Contains("7", raw);
        Assert.Contains("8", raw);
        Assert.Contains("9", raw);
    }
}
