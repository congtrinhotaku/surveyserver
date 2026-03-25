namespace SurveyService.Models
{
    public class UpdatePageRequest
    {
        public string Title { get; set; } = string.Empty;


        public int OrderIndex { get; set; }
    }
}