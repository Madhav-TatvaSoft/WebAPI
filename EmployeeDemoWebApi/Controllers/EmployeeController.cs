using EmployeeDemoWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeDemoWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly EmployeeDbContext _db;

    public EmployeeController(EmployeeDbContext db)
    {
        _db = db;
    }

    [Authorize]
    [HttpGet]
    [Route("getAllEmployees")]
    public IActionResult GetAllEmployees()
    {
        var employees = _db.Employees.ToList();
        if (employees == null || !employees.Any())
        {
            return Ok(new List<Employee>());
        }
        return Ok(employees);
    }

    [Authorize]
    [HttpGet]
    [Route("getEmployee/{id}")]
    public IActionResult GetById(int id)
    {
        var emp = _db.Employees.FirstOrDefault(e => e.Id == id);
        return emp == null ? NotFound() : Ok(emp);
    }

    [Authorize]
    [HttpPost]
    [Route("addEmployee")]
    public IActionResult CreateEmployee([FromBody] Employee employee)
    {
        if (employee == null)
        {
            return BadRequest("Employee data is null");
        }

        _db.Employees.Add(employee);
        _db.SaveChanges();
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    [Route("updateEmployee/{id}")]
    public IActionResult UpdateEmployee(int id, [FromBody] Employee employee)
    {
        if (employee == null && id <= 0)
        {
            return BadRequest("Employee data is invalid");
        }

        var existingEmployee = _db.Employees.FirstOrDefault(e => e.Id == id);
        if (existingEmployee == null)
        {
            return NotFound();
        }

        existingEmployee.Firstname = employee.Firstname;
        existingEmployee.Lastname = employee.Lastname;
        existingEmployee.Email = employee.Email;
        existingEmployee.Address = employee.Address;
        existingEmployee.Phonenumber = employee.Phonenumber;

        _db.SaveChanges();
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete]
    [Route("deleteEmployee/{id}")]
    public IActionResult DeleteEmployee(int id)
    {
        var employee = _db.Employees.FirstOrDefault(e => e.Id == id);
        if (employee == null)
        {
            return NotFound();
        }

        _db.Employees.Remove(employee);
        _db.SaveChanges();
        return NoContent();
    }

}