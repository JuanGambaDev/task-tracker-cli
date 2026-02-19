namespace TaskTracker.Models;

public class TaskItem
{
    public int Id { get; init; }
    public string Description { get; set; }  = string.Empty;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.ToDo;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }

}
