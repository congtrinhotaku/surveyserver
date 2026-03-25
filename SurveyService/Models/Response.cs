using System;
using System.Collections.Generic;

namespace SurveyService.Models;

public partial class Response
{
    public int Id { get; set; }

    public int? SurveyId { get; set; }

    public string? Token { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Survey? Survey { get; set; }
}
