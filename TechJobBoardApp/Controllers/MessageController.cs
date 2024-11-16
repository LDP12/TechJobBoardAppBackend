using FreelanceJobBoard.Data;
using FreelanceJobBoard.DTOs;
using FreelanceJobBoard.Models;
using FreelanceJobBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MessageController : ControllerBase
{
    private readonly AppDbContext _Context;
    private readonly NotificationService _NotificationService;
    
    [HttpPost("send")]
    [Authorize]
    public async Task<IActionResult> SendMessage(MessageDto messageDto)
    {
        var message = new Message
        {
            SenderId = int.Parse(User.Identity.Name),
            ReceiverId = messageDto.ReceiverId,
            Content = messageDto.Content
        };

        _Context.Messages.Add(message);
        await _Context.SaveChangesAsync();

        return Ok(new { Message = "Message sent successfully." });
    }

    [HttpGet("conversation/{receiverId}")]
    [Authorize]
    public async Task<IActionResult> GetConversation(int receiverId)
    {
        var userId = int.Parse(User.Identity.Name);

        var messages = await _Context.Messages
            .Where(m => (m.SenderId == userId && m.ReceiverId == receiverId) ||
                        (m.SenderId == receiverId && m.ReceiverId == userId))
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        return Ok(messages);
    }
}