using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaskTracker.Repositories;

public class JsonRepository<T> : IRepository<T>
{
    private readonly string _filePath;

    // Shared options: indent for readability + serialize enums as strings ("ToDo", "InProgress", "Done")
    // instead of integers (0, 1, 2), making the JSON file human-readable and easier to repair manually.
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public JsonRepository(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<List<T>> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return new List<T>();

        try
        {
            await using var stream = File.OpenRead(_filePath);
            return await JsonSerializer.DeserializeAsync<List<T>>(stream, SerializerOptions)
                   ?? new List<T>();
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new IOException(
                $"Access denied when reading file '{_filePath}'. " +
                $"Check file permissions.", ex);
        }
        catch (JsonException ex)
        {
            throw new IOException(
                $"The data file '{_filePath}' contains invalid JSON and cannot be read. " +
                $"Delete or repair the file and try again. Details: {ex.Message}", ex);
        }
        catch (IOException ex)
        {
            throw new IOException(
                $"I/O error occurred while reading file '{_filePath}': {ex.Message}", ex);
        }
    }

    public async Task SaveAsync(List<T> items)
    {
        try
        {
            await using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, items, SerializerOptions);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new IOException(
                $"Access denied when writing file '{_filePath}'. " +
                $"Check file permissions.", ex);
        }
        catch (IOException ex)
        {
            throw new IOException(
                $"I/O error occurred while writing file '{_filePath}': {ex.Message}", ex);
        }
    }
}
