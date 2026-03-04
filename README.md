# Task Tracker CLI

A command-line application to manage your personal task list. Add tasks, track their progress, mark them done, and keep everything stored locally in a human-readable JSON file — no database, no internet connection, no third-party libraries.

---

## Table of Contents

- [Project Overview](#project-overview)
- [Requirements](#requirements)
- [Installation](#installation)
- [Usage](#usage)
- [Error Handling](#error-handling)
- [Data Storage](#data-storage)
- [Architecture](#architecture)
- [Running the Tests](#running-the-tests)
- [Limitations and Future Improvements](#limitations-and-future-improvements)

---

## Project Overview

Task Tracker CLI is a .NET 8 console application built as a focused exercise in clean CLI design, layered architecture, file system interaction, input validation, and unit testing.

**Capabilities:**

- Add tasks with a description
- Update a task's description
- Delete tasks by ID
- Mark tasks as in-progress or done
- List all tasks, or filter by status (`todo`, `in-progress`, `done`)

**Design goals:**

- The application **never crashes** on bad input — every error surfaces as a clear, actionable message
- Business logic is **fully decoupled** from both the CLI and the file system
- The JSON data file is **human-readable and repairable** — status values are stored as strings, not integers
- Every public behaviour is **covered by unit tests**

---

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download) or later
- Any OS: Windows, macOS, Linux

---

## Installation

```bash
# 1. Clone the repository
git clone https://github.com/JuanGambaDev/task-tracker-cli.git
cd task-tracker-cli

# 2. Build
dotnet build

# 3. Run
dotnet run --project TaskTracker -- <command> [arguments]
```

### Optional: install as a local binary

```bash
dotnet publish TaskTracker -c Release -o ./publish
./publish/TaskTracker <command> [arguments]
```

> `tasks.json` is created automatically in the current working directory on the first write. It is excluded from version control via `.gitignore`.

---

## Usage

### Command reference

| Command | Description |
|---|---|
| `add "description"` | Add a new task |
| `list` | List all tasks |
| `list todo` | List tasks with status: todo |
| `list in-progress` | List tasks with status: in-progress |
| `list done` | List tasks with status: done |
| `update <id> "description"` | Update a task's description |
| `mark-in-progress <id>` | Mark a task as in-progress |
| `mark-done <id>` | Mark a task as done |
| `delete <id>` | Delete a task by ID |

---

### Add a task

```bash
dotnet run --project TaskTracker -- add "Buy groceries"
# Task added successfully (ID: 1)

dotnet run --project TaskTracker -- add "Write unit tests"
# Task added successfully (ID: 2)
```

---

### List tasks

```bash
# All tasks
dotnet run --project TaskTracker -- list

# Filter by status
dotnet run --project TaskTracker -- list todo
dotnet run --project TaskTracker -- list in-progress
dotnet run --project TaskTracker -- list done
```

**Example output:**

```
----- Task -----
Task id:     1
Description: Buy groceries
Status:      ToDo
Created At:  2024-03-01 10:00:00Z
----------------
----- Task -----
Task id:     2
Description: Write unit tests
Status:      InProgress
Created At:  2024-03-01 11:00:00Z
----------------
```

---

### Update a task description

```bash
dotnet run --project TaskTracker -- update 1 "Buy groceries and cook dinner"
# Task updated successfully (ID: 1)
```

---

### Mark a task as in-progress

```bash
dotnet run --project TaskTracker -- mark-in-progress 1
# Task marked as In Progress (ID: 1)
```

---

### Mark a task as done

```bash
dotnet run --project TaskTracker -- mark-done 1
# Task marked as Done (ID: 1)
```

---

### Delete a task

```bash
dotnet run --project TaskTracker -- delete 1
# Task deleted successfully (ID: 1)
```

---

### Help

Running with no arguments prints the full command reference:

```bash
dotnet run --project TaskTracker
```

```
Task Tracker CLI

Commands:
  add "task description"          Add a new task
  list                            List all tasks
  list todo                       List tasks with status: todo
  list in-progress                List tasks with status: in-progress
  list done                       List tasks with status: done
  update <id> "new description"   Update a task's description
  mark-in-progress <id>           Mark a task as in-progress
  mark-done <id>                  Mark a task as done
  delete <id>                     Delete a task
```

---

## Error Handling

The application handles all bad input gracefully. Errors are written to `stderr` and the process exits cleanly — no stack traces, no crashes.

| Scenario | Output |
|---|---|
| No arguments | Displays full help text |
| Missing argument | `Error: Not enough arguments. Usage: mark-in-progress <id>` |
| Non-integer task ID | `Error: Task id must be a valid integer, got 'abc'.` |
| Negative or zero ID | `Error: Task id must be greater than zero.` |
| Task not found | `Error: Task with id 99 not found.` |
| Unknown command | `Error: Unknown command 'fly'.` |
| Empty description | `Error: Task description cannot be empty.` |
| Invalid status filter | `Error: Invalid status filter 'xyz'. Valid values: todo \| in-progress \| done` |
| Corrupted JSON file | `Storage error: The data file 'tasks.json' contains invalid JSON and cannot be read. Delete or repair the file and try again.` |
| Permission denied | `Storage error: Access denied when reading file 'tasks.json'. Check file permissions.` |

---

## Data Storage

Tasks are stored in `tasks.json` in the **current working directory**. The file is created automatically on the first write and does not need to exist beforehand.

### Example file

```json
[
  {
    "Id": 1,
    "Description": "Buy groceries",
    "Status": "ToDo",
    "CreatedAt": "2024-03-01T10:00:00Z",
    "UpdatedAt": "2024-03-01T10:00:00Z"
  },
  {
    "Id": 2,
    "Description": "Write unit tests",
    "Status": "InProgress",
    "CreatedAt": "2024-03-01T11:00:00Z",
    "UpdatedAt": "2024-03-01T12:30:00Z"
  }
]
```

### Field reference

| Field | Type | Notes |
|---|---|---|
| `Id` | `int` | Auto-incremented. Never reused after deletion. |
| `Description` | `string` | Leading and trailing whitespace is trimmed automatically. |
| `Status` | `string` | One of: `ToDo`, `InProgress`, `Done`. Stored as a string for readability. |
| `CreatedAt` | `datetime` | UTC. Set once on creation, never modified. |
| `UpdatedAt` | `datetime` | UTC. Updated on every write operation. |

> **Tip:** The file is pretty-printed and fully human-readable. You can open it in any text editor to inspect, back up, or manually repair it. If it becomes corrupted, the application will report a storage error with enough detail to locate and fix the problem.

> **Note:** `tasks.json` is listed in `.gitignore` to prevent accidental commits of personal task data.

---

## Architecture

```
task-tracker-cli/
├── TaskTracker/
│   ├── Models/
│   │   ├── TaskItem.cs            # Task entity
│   │   └── TaskItemStatus.cs      # Enum: ToDo | InProgress | Done
│   ├── Repositories/
│   │   ├── IRepository.cs         # Generic interface: LoadAsync / SaveAsync
│   │   └── JsonRepository.cs      # File system implementation with JSON serialisation
│   ├── Services/
│   │   └── TaskService.cs         # All business logic
│   └── Program.cs                 # CLI entry point: argument parsing, routing, error handling
├── TaskTracker.Tests/
│   ├── TestBase.cs                # Per-test isolated temp file + IDisposable cleanup
│   ├── JsonRepositoryTests.cs     # Persistence, error paths, enum serialisation
│   └── TaskServiceTests.cs        # Full CRUD, status transitions, all boundary conditions
└── .gitignore
```

### Key design decisions

**Repository pattern (`IRepository<T>`)**
`TaskService` never touches the file system directly. It depends on `IRepository<T>`, which `JsonRepository<T>` implements. This boundary means the service is testable without mocking frameworks — tests simply point `JsonRepository` at a temp file. It also means swapping storage (e.g. SQLite, a remote API) requires no changes to business logic.

**Clean service layer**
`TaskService` accepts and returns domain types only (`TaskItemStatus`, `TaskItem`). It has zero knowledge of CLI commands, argument strings, or file paths. The translation from `"mark-in-progress"` to `TaskItemStatus.InProgress` happens in `Program.cs`, at the application boundary where it belongs.

**No `ITaskService` (by design)**
`ITaskService` is not present because it would be speculative abstraction. There is one service implementation, no DI container, and tests exercise the real service directly against a temp file repository. The interface will be introduced when a second consumer (e.g. a web API or GUI) requires it.

**Enum serialised as string**
`JsonStringEnumConverter` is applied globally via a shared `JsonSerializerOptions` instance in `JsonRepository`. This means the JSON file stores `"InProgress"` instead of `1` — legible to humans, easier to repair, and unambiguous if the enum order ever changes.

**Errors written to `stderr`**
All `Console.Error.WriteLine` calls go to `stderr`, leaving `stdout` clean for structured output. This is the correct behaviour for CLI tools and allows callers to pipe task output without mixing in error messages.

---

## Running the Tests

```bash
dotnet test
```

### What is covered

**`JsonRepositoryTests`**

| Test | Scenario |
|---|---|
| `LoadAsync_ReturnsEmptyList_WhenFileDoesNotExist` | No file present |
| `LoadAsync_ReturnsEmptyList_WhenFileIsEmpty` | Empty file → IOException |
| `LoadAsync_ThrowsIOException_WhenFileContainsCorruptedJson` | Corrupt JSON → clear error message |
| `LoadAsync_ThrowsIOException_WhenFileContainsWrongJsonType` | Valid JSON, wrong shape |
| `SaveAndLoadAsync_PersistsDataCorrectly` | Full round-trip |
| `SaveAsync_OverwritesPreviousData` | Second save replaces first |
| `SaveAndLoadAsync_PreservesEmptyList` | Empty list round-trip |
| `SaveAsync_SerializesTaskItemStatusAsString` | Enum stored as `"InProgress"`, not `1` |
| `SaveAndLoadAsync_RoundTrips_EnumValuesCorrectly` | Enum survives save and load |

**`TaskServiceTests`**

| Area | Tests |
|---|---|
| Add | Incremented ID, empty description throws, description trimmed, default status ToDo |
| List | Returns all, filters by status, returns empty list |
| Update description | Updates correctly, throws on zero ID, negative ID, empty description, missing task |
| Update status | Sets InProgress, sets Done, reverts to ToDo, throws on zero ID, negative ID, missing task |
| Delete | Deletes and returns task, removes from list, throws on zero ID, negative ID, missing task |

### Test isolation

Every test class extends `TestBase`, which generates a unique temp file path per test using `Guid.NewGuid()` and deletes it in `Dispose()`. Tests never share state and can run safely in parallel.

---

## Limitations and Future Improvements

### Current limitations

- **Single user, single list.** All tasks share one `tasks.json` in the working directory. There is no concept of projects, workspaces, or users.
- **No priorities or due dates.** Tasks carry only a description and a status.
- **No search or pagination.** `list` always prints every matching task with no keyword filtering or paging.
- **No undo.** `delete` is permanent and has no confirmation prompt.
- **Plain text output.** No colour, no table formatting, no terminal width awareness.

### Planned improvements

- [ ] `--priority` flag: `low`, `medium`, `high`
- [ ] `due-date` field with overdue indicator on `list`
- [ ] `search <keyword>` command to filter by description
- [ ] Confirmation prompt before `delete`
- [ ] ANSI colour output
- [ ] Support for multiple named lists (`--list work`, `--list personal`)
- [ ] Publish as a global .NET tool (`dotnet tool install -g task-cli`)
- [ ] Introduce `ITaskService` and a DI container when a second consumer (web API, GUI) is added

---

## License

This project is open source and available under the [MIT License](LICENSE).
