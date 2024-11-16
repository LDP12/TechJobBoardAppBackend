using FreelanceJobBoard.Models;

public class Application
{
    public int ApplicationId { get; set; }
    public int JobId { get; set; }
    public int FreelancerId { get; set; }
    public string ApplicationText { get; set; }
    public ApplicationStatusEnum Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsBoosted { get; set; } = false;
}