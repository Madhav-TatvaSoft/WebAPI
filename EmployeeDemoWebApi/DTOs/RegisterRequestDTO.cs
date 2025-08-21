namespace EmployeeDemoWebApi.DTOs;

public class RegisterRequestDTO
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
    public int RoleId { get; set; }
}
