using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FreelanceJobBoard.DTOs;
using FreelanceJobBoard.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using FreelanceJobBoard.Data;
using FreelanceJobBoard.DTOs;
using FreelanceJobBoard.Services;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JobsController : ControllerBase
{
    private readonly AppDbContext _Context;
    private readonly NotificationService _NotificationService;

    public JobsController(AppDbContext context, NotificationService notificationService)
    {
        _Context = context;
        _NotificationService = notificationService;
    }

    [HttpPost("post")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> PostJob(JobDto jobDto)
    {
        var employerIdClaim = User.Identity.Name;
        if (string.IsNullOrEmpty(employerIdClaim))
        {
            return Unauthorized("User ID is missing. Make sure you're authenticated.");
        }

        var employerId = int.Parse(employerIdClaim);

        var job = new Job
        {
            Title = jobDto.Title,
            Description = jobDto.Description,
            Category = jobDto.CategoryEnum.ToString(),
            Budget = jobDto.Budget,
            EmployerId = employerId,
            CreatedAt = DateTime.UtcNow,
            IsFeatured = jobDto.IsFeatured
        };

        _Context.Jobs.Add(job);
        await _Context.SaveChangesAsync();

        return Ok(new { JobId = job.JobId, Message = "Job posted successfully." });
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchJobs([FromQuery] JobSearchDto searchDto, int page = 1, int pageSize = 10)
    {
        IQueryable<Job> query = _Context.Jobs;

        if (searchDto.Category.HasValue)
        {
            var categoryValue = searchDto.Category.Value.ToString();
            query = query.Where(j => j.Category == categoryValue);
        }

        if (!string.IsNullOrEmpty(searchDto.Keywords))
        {
            query = query.Where(j => j.Title.Contains(searchDto.Keywords)
                                     || j.Description.Contains(searchDto.Keywords)
                                     || j.Category.Contains(searchDto.Keywords));
        }

        query = query.OrderByDescending(j => j.IsFeatured)
            .ThenByDescending(j => j.CreatedAt);

        var jobs = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(jobs);
    }

    [HttpPost("apply/{jobId}")]
    [Authorize(Roles = "Freelancer")]
    public async Task<IActionResult> ApplyToJob(int jobId, ApplicationDto applicationDto)
    {
        var freelancerIdClaim = User.Identity.Name;
        if (string.IsNullOrEmpty(freelancerIdClaim))
        {
            return Unauthorized("User ID is missing. Make sure you're authenticated.");
        }

        var freelancerId = int.Parse(freelancerIdClaim);

        var job = await _Context.Jobs.FindAsync(jobId);
        if (job == null)
        {
            return NotFound("Job not found.");
        }

        var existingApplication = await _Context.Applications
            .AnyAsync(a => a.JobId == jobId && a.FreelancerId == freelancerId);

        if (existingApplication)
        {
            return BadRequest("You have already applied for this job.");
        }

        var application = new Application
        {
            JobId = jobId,
            FreelancerId = freelancerId,
            ApplicationText = applicationDto.ApplicationText,
            Status = ApplicationStatusEnum.Received,
            CreatedAt = DateTime.UtcNow
        };

        _Context.Applications.Add(application);
        await _Context.SaveChangesAsync();

        var employerId = job.EmployerId;
        await _NotificationService.CreateNotification(employerId,
            $"New application received for your job: {job.Title}");

        return Ok(new { ApplicationId = application.ApplicationId, Message = "Application submitted successfully." });
    }

    [HttpGet("applications/{jobId}")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetApplications(int jobId)
    {
        var applications = await _Context.Applications
            .Where(a => a.JobId == jobId)
            .ToListAsync();

        return Ok(applications);
    }

    [HttpPut("applications/{applicationId}/status")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> UpdateApplicationStatus(int applicationId, [FromBody] UpdateStatusDto statusDto)
    {
        var application = await _Context.Applications.FindAsync(applicationId);
        if (application == null)
            return NotFound("Application not found.");

        var job = await _Context.Jobs.FindAsync(application.JobId);
        if (job == null || job.EmployerId != int.Parse(User.Identity.Name))
            return Unauthorized("You are not authorized to update this application.");

        application.Status = statusDto.Status;
        await _Context.SaveChangesAsync();

        var applicantId = application.FreelancerId;
        await _NotificationService.CreateNotification(applicantId,
            $"Your application for '{job.Title}' has been updated to: {statusDto.Status}");

        return Ok(new { Message = "Application status updated and applicant notified successfully." });
    }

    [HttpGet("my-jobs")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetEmployerJobs()
    {
        var employerIdClaim = User.Identity.Name;
        if (string.IsNullOrEmpty(employerIdClaim))
        {
            return Unauthorized("User ID is missing. Make sure you're authenticated.");
        }

        var employerId = int.Parse(employerIdClaim);

        var jobs = await _Context.Jobs
            .Where(j => j.EmployerId == employerId)
            .Select(j => new
            {
                j.JobId,
                j.Title,
                j.Description,
                j.Category,
                j.Budget,
                Applications = _Context.Applications
                    .Where(a => a.JobId == j.JobId)
                    .Select(a => new
                    {
                        a.ApplicationId,
                        a.FreelancerId,
                        a.ApplicationText,
                        a.Status,
                        a.CreatedAt
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(jobs);
    }
}