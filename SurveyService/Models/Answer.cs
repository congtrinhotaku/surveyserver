using System;
using System.Collections.Generic;

namespace SurveyService.Models;

public partial class Answer
{
    public int Id { get; set; }

    public int? ResponseId { get; set; }

    public int? QuestionId { get; set; }

    public string? AnswerText { get; set; }

    public double? AnswerNumber { get; set; }

    public DateTime? AnswerDate { get; set; }

    public virtual ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();

    public virtual Question? Question { get; set; }

    public virtual Response? Response { get; set; }
}
