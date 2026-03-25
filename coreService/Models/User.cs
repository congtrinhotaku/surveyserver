using System;
using System.Collections.Generic;

namespace coreService.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
