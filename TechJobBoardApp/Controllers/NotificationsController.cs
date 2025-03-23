using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelanceJobBoard.Data;

namespace FreelanceJobBoard.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _Context;

    public NotificationsController(AppDbContext context)
    {
        _Context = context;
    }

    [HttpGet("get-all-notifications")]
    [Authorize]
    public async Task<IActionResult> GetUserNotifications()
    {
        var userId = int.Parse(User.Identity.Name);
        var notifications = await _Context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpPut("{notificationId}/mark-as-read")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        var notification = await _Context.Notifications.FindAsync(notificationId);
        if (notification == null || notification.UserId != int.Parse(User.Identity.Name))
            return NotFound("Notification not found.");

        notification.IsRead = true;
        await _Context.SaveChangesAsync();

        return Ok(new { Message = "Notification marked as read." });
    }
}