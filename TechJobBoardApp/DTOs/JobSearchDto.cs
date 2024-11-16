using FreelanceJobBoard.Models;

namespace FreelanceJobBoard.DTOs;

public class JobSearchDto
{
    public string? Keywords { get; set; }
    public JobCategoryEnum? Category { get; set; }
}