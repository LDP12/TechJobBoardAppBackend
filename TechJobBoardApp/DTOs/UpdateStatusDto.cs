using FreelanceJobBoard.Models;

namespace FreelanceJobBoard.DTOs;

public class UpdateStatusDto
{
    public ApplicationStatusEnum Status { get; set; }
}