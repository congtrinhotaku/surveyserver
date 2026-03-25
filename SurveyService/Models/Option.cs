using System;
using System.Collections.Generic;

namespace SurveyService.Models;

public partial class Option
{
    public int Id { get; set; }

    public int? QuestionId { get; set; }

    public string? OptionText { get; set; }

    public int? OrderIndex { get; set; }

    public virtual ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();

    public virtual Question? Question { get; set; }
}
