using System;
using System.Collections.Generic;

namespace SurveyService.Models;

public partial class QuestionType
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
