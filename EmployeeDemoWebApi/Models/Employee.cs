using System;
using System.Collections.Generic;

namespace EmployeeDemoWebApi.Models;

public partial class Employee
{
    public int Id { get; set; }

    public string Firstname { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public string Address { get; set; } = null!;

    public long Phonenumber { get; set; }

    public string Email { get; set; } = null!;
}
