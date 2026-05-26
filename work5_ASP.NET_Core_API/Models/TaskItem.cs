namespace work5_ASP.NET_Core_API.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "todo";
    public int Priority { get; set; }
    public int OwnerId { get; set; }
}
