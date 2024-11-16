namespace FreelanceJobBoard.Models;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public RoleCategoryEnum Role { get; set; }
    public bool IsPremium { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsBoosted { get; set; } = false;
}