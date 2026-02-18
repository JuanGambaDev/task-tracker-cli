namespace TaskTracker.Models;

public class TaskItem
{
    public int Id { get; init; }
    public string Description { get; set; }  = string.Empty;
    public TaskStatus Status { get; set; } = TaskStatus.ToDo;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }

}
