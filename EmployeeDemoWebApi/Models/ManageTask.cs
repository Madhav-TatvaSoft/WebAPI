using System;
using System.Collections.Generic;

namespace EmployeeDemoWebApi.Models;

public partial class ManageTask
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime? Duedate { get; set; }

    public string Category { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public int Userid { get; set; }

    public bool Iscompleted { get; set; }

    public bool Isdeleted { get; set; }

    public virtual User User { get; set; } = null!;
}
