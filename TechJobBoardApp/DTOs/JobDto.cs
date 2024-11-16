using FreelanceJobBoard.Models;

namespace FreelanceJobBoard.DTOs;

public class JobDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public JobCategoryEnum CategoryEnum { get; set; }
    public decimal Budget { get; set; }
    public bool IsFeatured { get; set; } = false; 
}