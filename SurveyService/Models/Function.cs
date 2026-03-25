using System;
using System.Collections.Generic;

namespace SurveyService.Models;

public partial class Function
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
