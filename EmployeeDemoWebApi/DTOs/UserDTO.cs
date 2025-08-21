namespace EmployeeDemoWebApi.DTOs;

public class UserDTO
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
}
