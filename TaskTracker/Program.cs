using TaskTracker.Models;
using TaskTracker.Repositories;
using TaskTracker.Services;

var repository = new JsonRepository<TaskItem>("tasks.json");
var service = new TaskService(repository);

try
{
    if (args.Length == 0)
        throw new ArgumentException("No command provided.");

    switch (args[0].ToLower())
    {
        case "add":
            var task = await service.AddTaskAsync(args[1]);
            Console.WriteLine($"Task added successfully (ID: {task.Id})");
            break;

        default:
            Console.WriteLine("Unknown command.");
            break;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
