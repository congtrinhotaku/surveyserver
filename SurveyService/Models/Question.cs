using System;
using System.Collections.Generic;

namespace SurveyService.Models;

public partial class Question
{
    public int Id { get; set; }

    public int? PageId { get; set; }

    public string? QuestionText { get; set; }

    public int? QuestionTypeId { get; set; }

    public bool? IsRequired { get; set; }

    public int? OrderIndex { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();

    public virtual Page? Page { get; set; }

    public virtual QuestionType? QuestionType { get; set; }
}
