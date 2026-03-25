using System;
using System.Collections.Generic;

namespace SurveyService.Models;

public partial class AnswerOption
{
    public int Id { get; set; }

    public int? AnswerId { get; set; }

    public int? OptionId { get; set; }

    public virtual Answer? Answer { get; set; }

    public virtual Option? Option { get; set; }
}
