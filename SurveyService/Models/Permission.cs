using System;
using System.Collections.Generic;

namespace SurveyService.Models;

public partial class Permission
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int FunctionId { get; set; }

    public string HttpMethod { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual Function Function { get; set; } = null!;

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
