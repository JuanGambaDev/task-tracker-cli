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
            EnsureArgs(args, minArgs: 2, usage: "add \"task description\"");
            var createdTask = await service.AddTaskAsync(args[1]);
            Console.WriteLine($"Task added successfully (ID: {createdTask.Id})");
            break;
        }

        case "list":
        {
            TaskItemStatus? status = null;

            if (args.Length > 1)
                status = ParseStatus(args[1]);   // ArgumentException handled in outer catch

            var tasks = await service.GetTasksAsync(status);

            if (!tasks.Any())
            {
                Console.WriteLine("No tasks found.");
                break;
            }

            foreach (var task in tasks)
            {
                Console.WriteLine("----- Task -----");
                Console.WriteLine($"Task id:     {task.Id}");
                Console.WriteLine($"Description: {task.Description}");
                Console.WriteLine($"Status:      {task.Status}");
                Console.WriteLine($"Created At:  {task.CreatedAt:u}");
                Console.WriteLine($"Last Update: {task.UpdatedAt:u}");
                Console.WriteLine("----------------");
            }
            break;
        }

        case "update":
        {
            EnsureArgs(args, minArgs: 3, usage: "update <id> \"new description\"");

            if (!int.TryParse(args[1], out int idUpdate))
            {
                Console.Error.WriteLine($"Error: Task id must be a valid integer, got '{args[1]}'.");
                return;
            }

            var updatedTask = await service.UpdateTaskDescriptionAsync(idUpdate, args[2]);
            Console.WriteLine($"Task updated successfully (ID: {updatedTask.Id})");
            break;
        }

        case "mark-in-progress":
        {
            EnsureArgs(args, minArgs: 2, usage: "mark-in-progress <id>");

            if (!int.TryParse(args[1], out int idProgress))
            {
                Console.Error.WriteLine($"Error: Task id must be a valid integer, got '{args[1]}'.");
                return;
            }

            var updatedTask = await service.UpdateStatusTaskAsync(args[0], idProgress);
            Console.WriteLine($"Task marked as In Progress (ID: {updatedTask.Id})");
            break;
        }

        case "mark-in-done":
        {
            EnsureArgs(args, minArgs: 2, usage: "mark-in-done <id>");

            if (!int.TryParse(args[1], out int idDone))
            {
                Console.Error.WriteLine($"Error: Task id must be a valid integer, got '{args[1]}'.");
                return;
            }

            var updatedTask = await service.UpdateStatusTaskAsync(args[0], idDone);
            Console.WriteLine($"Task marked as Done (ID: {updatedTask.Id})");
            break;
        }

        case "delete":
        {
            EnsureArgs(args, minArgs: 2, usage: "delete <id>");

            if (!int.TryParse(args[1], out int idDelete))
            {
                Console.Error.WriteLine($"Error: Task id must be a valid integer, got '{args[1]}'.");
                return;
            }

            var deletedTask = await service.DeleteTaskByIdAsync(idDelete);
            Console.WriteLine($"Task deleted successfully (ID: {deletedTask.Id})");
            break;
        }

        default:
            Console.Error.WriteLine($"Error: Unknown command '{args[0]}'.");
            ShowHelp();
            break;
    }
}
catch (ArgumentException ex)
{
    // Covers: missing args, bad status filter, empty description, non-positive id.
    Console.Error.WriteLine($"Error: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    // Covers: task not found.
    Console.Error.WriteLine($"Error: {ex.Message}");
}
catch (IOException ex)
{
    // Covers: corrupted JSON, permission denied, generic I/O failures.
    Console.Error.WriteLine($"Storage error: {ex.Message}");
}
catch (Exception ex)
{
    // Last-resort guard — app must never surface a raw stack trace to the user.
    Console.Error.WriteLine($"Unexpected error: {ex.Message}");
}

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

static void EnsureArgs(string[] args, int minArgs, string usage)
{
    if (args.Length < minArgs)
        throw new ArgumentException(
            $"Not enough arguments. Usage: {usage}");
}

static TaskItemStatus ParseStatus(string input)
{
    return input.ToLower() switch
    {
        "todo"        => TaskItemStatus.ToDo,
        "in-progress" => TaskItemStatus.InProgress,
        "done"        => TaskItemStatus.Done,
        _             => throw new ArgumentException(
                             $"Invalid status filter '{input}'. Valid values: todo | in-progress | done")
    };
}

static void ShowHelp()
{
    Console.WriteLine("""
Task Tracker CLI

Commands:
  add "task description"          Add a new task
  list                            List all tasks
  list todo                       List tasks with status: todo
  list in-progress                List tasks with status: in-progress
  list done                       List tasks with status: done
  update <id> "new description"   Update a task's description
  mark-in-progress <id>           Mark a task as in-progress
  mark-in-done <id>               Mark a task as done
  delete <id>                     Delete a task
""");
}
