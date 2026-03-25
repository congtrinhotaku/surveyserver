using System;
using System.Collections.Generic;

namespace SurveyService.Models;

public partial class Survey
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? CreatorUser { get; set; }

    public string? CreatorPassword { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Page> Pages { get; set; } = new List<Page>();

    public virtual ICollection<Response> Responses { get; set; } = new List<Response>();
}
