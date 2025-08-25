using EmployeeDemoWebApi.DTOs;
using EmployeeDemoWebApi.Models;
using EmployeeDemoWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDemoWebApi.Controllers;


[ApiController]
[Route("[controller]")]
public class TasksController : ControllerBase
{
    private readonly EmployeeDbContext _context;
    private readonly JWTService _jwtService;

    public TasksController(EmployeeDbContext context, JWTService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    private int GetUserId()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var userIdClaim = _jwtService.GetClaimValue(token, "UserId");
        if (userIdClaim == null)
        {
            throw new UnauthorizedAccessException("UserId claim not found in token.");
        }


        return int.Parse(userIdClaim);
    }

    [HttpGet]
    [Route("getAllTasks")]
    public async Task<IActionResult> GetAllTasks()
    {
        var userId = GetUserId();
        var tasks = await _context.ManageTasks.Where(t => t.Userid == userId && !t.Isdeleted).OrderBy(t => t.Id).ToListAsync();
        if (tasks == null || !tasks.Any())
        {
            return Ok(new List<ManageTask>());
        }
        return Ok(tasks);
    }

    [HttpGet]
    [Route("taskDetails/{id}")]
    public async Task<IActionResult> GetTask(int id)
    {
        var task = await _context.ManageTasks.FindAsync(id);
        if (task == null || task.Isdeleted)
            return NotFound();

        return Ok(task);
    }

    [HttpPost]
    [Route("createTask")]
    public async Task<IActionResult> CreateTask([FromBody] TaskCreateDTO dto)
    {
        try
        {
            var task = new ManageTask
            {
                Title = dto.Title,
                Description = dto.Description,
                Duedate = dto.DueDate,
                Category = dto.Category,
                Priority = dto.Priority,
                Userid = dto.UserId,
                Iscompleted = false,
                Isdeleted = false
            };

            _context.ManageTasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error creating task", error = ex.Message });
        }
    }

    [HttpPut]
    [Route("updateTask/{id}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskUpdateDTO dto)
    {
        if (id != dto.Id) return BadRequest("Task ID mismatch");

        var task = await _context.ManageTasks.FindAsync(id);
        if (task == null || task.Isdeleted)
            return NotFound();

        // Map updated values
        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Duedate = dto.DueDate;
        task.Category = dto.Category;
        task.Priority = dto.Priority;
        task.Iscompleted = dto.IsCompleted;

        _context.Entry(task).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete]
    [Route("deleteTask/{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var existing = await _context.ManageTasks.FindAsync(id);
        if (existing == null || existing.Userid != GetUserId()) return NotFound();

        existing.Isdeleted = true;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Task deleted" });
    }
}