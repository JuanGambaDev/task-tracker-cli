namespace TaskTracker.Tests;

/// <summary>
/// Provides a unique, isolated temp file path for each test and cleans it up afterward.
/// </summary>
public abstract class TestBase : IDisposable
{
    protected string TempFilePath { get; } = Path.Combine(
        Path.GetTempPath(),
        $"tasktracker_test_{Guid.NewGuid()}.json");

    public void Dispose()
    {
        if (File.Exists(TempFilePath))
            File.Delete(TempFilePath);
    }
}
