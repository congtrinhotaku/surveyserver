    public class CreateQuestionRequest
    {
        public int PageId { get; set; }
        public string QuestionText { get; set; }
        public int QuestionTypeId { get; set; }
        public bool IsRequired { get; set; }
        public int OrderIndex { get; set; }

        public List<string>? Options { get; set; }
    }