namespace EmployeeDemoWebApi.DTOs;

public class LoginResponseDTO
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public string? Token { get; set; }
    public UserDTO? User { get; set; }
}
