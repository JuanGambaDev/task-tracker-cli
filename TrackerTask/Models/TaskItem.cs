namespace TrackerTask.Models;

public sealed class TaskItem
{
    public int Id { get; init; }
    public string Description { get; private set; }
    public TaskStatus Status { get; private set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; private set; }

}
