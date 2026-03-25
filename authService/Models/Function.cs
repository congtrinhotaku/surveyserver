using System;
using System.Collections.Generic;

namespace authService.Models;

public partial class Function
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Path { get; set; }

    public int? ParentId { get; set; }

    public string? Icon { get; set; }

    public int? OrderIndex { get; set; }

    public bool? IsMenu { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Function> InverseParent { get; set; } = new List<Function>();

    public virtual Function? Parent { get; set; }

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
