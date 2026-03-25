public class SubmitSurveyRequest
{
    public List<AnswerDto> Answers { get; set; }
}

public class AnswerDto
{
    public int QuestionId { get; set; }

    public string? AnswerText { get; set; }
    public double? AnswerNumber { get; set; }
    public DateTime? AnswerDate { get; set; }

    public List<int>? OptionIds { get; set; }
}