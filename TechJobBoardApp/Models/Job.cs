namespace FreelanceJobBoard.Models;

public class Job
{
    public int JobId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public decimal Budget { get; set; }
    public int EmployerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsFeatured { get; set; } = false;
}
