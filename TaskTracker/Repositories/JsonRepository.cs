using System.Text.Json;

namespace TaskTracker.Repositories;

public class JsonRepository<T>
{
    private readonly string _filePath;

    public JsonRepository(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<List<T>> LoadAsync()
    {
        try
        {
            if (!File.Exists(_filePath))
                return new List<T>();

            await using var stream = File.OpenRead(_filePath);

            return await JsonSerializer.DeserializeAsync<List<T>>(stream)
                   ?? new List<T>();
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new IOException(
                $"Access denied when reading file '{_filePath}'.", ex);
        }
        catch (JsonException ex)
        {
            throw new IOException(
                $"Invalid JSON format in file '{_filePath}'.", ex);
        }
        catch (IOException ex)
        {
            throw new IOException(
                $"I/O error occurred while reading file '{_filePath}'.", ex);
        }
    }

    public async Task SaveAsync(List<T> items)
    {
        try
        {
            await using var stream = File.Create(_filePath);

            await JsonSerializer.SerializeAsync(
                stream,
                items,
                new JsonSerializerOptions { WriteIndented = true }
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new IOException(
                $"Access denied when writing file '{_filePath}'.", ex);
        }
        catch (IOException ex)
        {
            throw new IOException(
                $"I/O error occurred while writing file '{_filePath}'.", ex);
        }
    }
}
