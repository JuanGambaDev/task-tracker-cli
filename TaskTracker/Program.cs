using TaskTracker.Models;
using TaskTracker.Repositories;
using TaskTracker.Services;

var repository = new JsonRepository<TaskItem>("tasks.json");
var service = new TaskService(repository);

try
{
    if (args.Length == 0)
    {
        ShowHelp();
        return;
    }

    switch (args[0].ToLower())
    {
        case "add":
        {
            EnsureArgs(args, 2, "add \"task description\"");
            var createdTask = await service.AddTaskAsync(args[1]);
            Console.WriteLine($"Task added successfully (ID: {createdTask.Id})");
            break;
        }

        case "list":
        {
            TaskItemStatus? status = null;

            if (args.Length > 1)
                status = ParseStatus(args[1]);

            var tasks = await service.GetAllTasksAsync(status);

            if (!tasks.Any())
            {
                Console.WriteLine("No tasks found.");
                break;
            }

            foreach (TaskItem task in tasks)
            {
                Console.WriteLine("----- Task -----");
                Console.WriteLine($"Task id: {task.Id}");
                Console.WriteLine($"Description: {task.Description}");
                Console.WriteLine($"Status: {task.Status}");
                Console.WriteLine($"Created At: {task.CreatedAt}");
                Console.WriteLine("----------------");
            }
            break;
        }

        default:
            Console.WriteLine("Unknown command.");
            ShowHelp();
            break;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

static void EnsureArgs(string[] args, int minArgs, string usage)
{
    if (args.Length < minArgs)
        throw new ArgumentException($"Invalid arguments. Usage: {usage}");
}

static TaskItemStatus ParseStatus(string input)
{
    return input.ToLower() switch
    {
        "todo" => TaskItemStatus.ToDo,
        "in-progress" => TaskItemStatus.InProgress,
        "done" => TaskItemStatus.Done,
        _ => throw new ArgumentException(
            "Invalid status. Use: todo | in-progress | done")
    };
}

static void ShowHelp()
{
    Console.WriteLine("""
Task Tracker CLI

Commands:
  add "task description"
  list
  list todo
  list in-progress
  list done
""");
}
