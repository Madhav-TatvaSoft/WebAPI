using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EmployeeDemoWebApi.DTOs;
using EmployeeDemoWebApi.Models;
using EmployeeDemoWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeDemoWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly EmployeeDbContext _db;
    private readonly JWTService _jwtService;
    private readonly IConfiguration _config;

    public AuthController(EmployeeDbContext db, JWTService jwtService, IConfiguration config)
    {
        _db = db;
        _jwtService = jwtService;
        _config = config;
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
    {
        if (await _db.Users.AnyAsync(u => u.Username.ToLower().Trim() == request.UserName.ToLower().Trim()))
        {
            return BadRequest(new { message = "Username already exists" });
        }

        if (await _db.Users.AnyAsync(u => u.Email.ToLower().Trim() == request.Email.ToLower().Trim()))
        {
            return BadRequest(new { message = "Email already exists" });
        }

        if (request.Password != request.ConfirmPassword)
        {
            return BadRequest(new { message = "Passwords do not match" });
        }

        var passwordHash = HashPassword(request.Password);

        var user = new User
        {
            Username = request.UserName,
            Email = request.Email,
            Password = passwordHash,
            Roleid = request.RoleId
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { message = "User registered successfully" });
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
    {
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !VerifyPassword(request.Password, user.Password))
        {
            return BadRequest(new LoginResponseDTO
            {
                Success = false,
                Message = "Invalid credentials",
                Token = null,
                User = null
            });
        }

        var token = _jwtService.GenerateToken(user);

        if (token == null)
        {
            return BadRequest(new
            {
                Message = "Token generation failed"
            });
        }

        Response.Cookies.Append("AuthToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_config["JwtConfig:Duration"]))
        });

        return Ok(new { message = "Login successful", token });

    }

    [HttpGet]
    [Route("getUser")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _db.Users
            .Include(u => u.Role)
            .Select(u => new UserDTO
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                RoleId = u.Roleid,
                RoleName = u.Role!.Name
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet]
    [Route("validate")]
    public IActionResult ValidateToken()
    {
        var token = Request.Headers.Cookie.ToString().Split(';')
            .FirstOrDefault(c => c.Trim().StartsWith("AuthToken="))?
            .Split('=')[1];

        if (string.IsNullOrEmpty(token))
            return Unauthorized(new { message = "Token is missing" });

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            // var principal = tokenHandler.ValidateToken(token, _tokenValidationParams, out var validatedToken);
            // return Ok(new { message = "Token is valid", user = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value });
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var username = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var userId = jwt.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            return Ok(new { valid = true, username, userId });
        }
        catch (SecurityTokenExpiredException)
        {
            return Unauthorized(new { message = "Token has expired" });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = "Invalid token", error = ex.Message });
        }
    }

    // Methods
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }

}