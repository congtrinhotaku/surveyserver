using System;
using System.Collections.Generic;

namespace coreService.Models;

public partial class Menu
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Path { get; set; }

    public int? ParentId { get; set; }

    public string? Icon { get; set; }

    public int? OrderIndex { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? FunctionId { get; set; }

    public virtual Function? Function { get; set; }
}
