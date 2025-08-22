namespace EmployeeDemoWebApi.DTOs;

public class TaskUpdateDTO
{
    public int Id { get; set; }  
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime? DueDate { get; set; }
    public string Category { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public bool IsCompleted { get; set; }
}
