namespace SurveyService.Models
{
    public class CreatePageRequest
    {
        public int SurveyId { get; set; }

        public string Title { get; set; } = string.Empty;


        public int OrderIndex { get; set; }
    }
}