    public class UpdateQuestionRequest
    {

        public string QuestionText { get; set; }
        public int QuestionTypeId { get; set; }
        public bool IsRequired { get; set; }
        public int OrderIndex { get; set; }

        public List<OptionDto>? Options { get; set; }
    }
    public class OptionDto
    {
        public string OptionText { get; set; }
    }