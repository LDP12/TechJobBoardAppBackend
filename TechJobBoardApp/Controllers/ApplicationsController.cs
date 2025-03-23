using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FreelanceJobBoard.Models;
using FreelanceJobBoard.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ApplicationsController : ControllerBase
{
    private readonly AppDbContext _Context;

    public ApplicationsController(AppDbContext context)
    {
        _Context = context;
    }

    [HttpGet("my-applications")]
    [Authorize(Roles = "Freelancer")]
    public async Task<IActionResult> GetFreelancerApplications()
    {
        var freelancerIdClaim = User.Identity.Name;
        if (string.IsNullOrEmpty(freelancerIdClaim))
        {
            return Unauthorized("User ID is missing. Make sure you're authenticated.");
        }

        var freelancerId = int.Parse(freelancerIdClaim);

        var applications = await _Context.Applications
            .Where(a => a.FreelancerId == freelancerId)
            .Select(a => new
            {
                a.ApplicationId,
                JobTitle = _Context.Jobs
                    .Where(j => j.JobId == a.JobId)
                    .Select(j => j.Title)
                    .FirstOrDefault(),
                a.ApplicationText,
                a.Status,
                a.CreatedAt,
                a.IsBoosted
            })
            .ToListAsync();

        return Ok(applications);
    }

    [HttpPost("boost/{applicationId}")]
    [Authorize(Roles = "Freelancer")]
    public async Task<IActionResult> BoostApplication(int applicationId)
    {
        var application = await _Context.Applications.FindAsync(applicationId);
        if (application == null)
            return NotFound("Application not found.");

        application.IsBoosted = true;
        await _Context.SaveChangesAsync();

        return Ok(new { Message = "Application boosted successfully." });
    }
}