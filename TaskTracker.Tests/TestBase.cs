using System;
using System.IO;

namespace TaskTracker.Tests;

public abstract class TestBase : IDisposable
{
    protected string TempFilePath { get; }

    protected TestBase()
    {
        TempFilePath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.json");
    }

    public void Dispose()
    {
        if (File.Exists(TempFilePath))
            File.Delete(TempFilePath);
    }
}