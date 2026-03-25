using System;
using System.Collections.Generic;

namespace SurveyService.Models;

public partial class Page
{
    public int Id { get; set; }

    public int? SurveyId { get; set; }

    public string? Title { get; set; }

    public int? OrderIndex { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual Survey? Survey { get; set; }
}
