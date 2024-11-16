namespace FreelanceJobBoard.Models;

public class Subscription
{
    public int SubscriptionId { get; set; }
    public int UserId { get; set; }
    public string SubscriptionType { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
}