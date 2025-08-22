namespace EmployeeDemoWebApi.DTOs;

public class TaskCreateDTO
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime? DueDate { get; set; }
    public string Category { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public int UserId { get; set; }
}
