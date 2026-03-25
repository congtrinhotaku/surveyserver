using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurveyService.Models;
using SurveyService.Services;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace SurveyService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicSurveyController : ControllerBase
    {
        private readonly SurveyDbContext _context;
        private readonly AuthClient _authClient;

        public PublicSurveyController(SurveyDbContext context, AuthClient authClient)
        {
            _context = context;
            _authClient = authClient;
        }

        // =========================
        // GET FULL SURVEY
        // =========================
        [HttpGet("{surveyId}")]
        public async Task<IActionResult> GetById(int surveyId)
            {
                var survey = await _context.Surveys
                    .Include(x => x.Pages)
                        .ThenInclude(p => p.Questions)
                            .ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(x => x.Id == surveyId && x.IsActive == true);

                if (survey == null)
                    return NotFound();

                var result = new
                {
                    survey.Id,
                    survey.Title,
                    survey.Description,
                    Pages = survey.Pages
                        .OrderBy(p => p.OrderIndex)
                        .Select(p => new
                        {
                            p.Id,
                            p.Title,
                            p.OrderIndex,
                            Questions = p.Questions
                                .OrderBy(q => q.OrderIndex)
                                .Select(q => new
                                {
                                    q.Id,
                                    q.QuestionText,
                                    q.QuestionTypeId,
                                    q.IsRequired,
                                    q.OrderIndex,
                                    Options = q.Options
                                        .OrderBy(o => o.OrderIndex)
                                        .Select(o => new
                                        {
                                            o.Id,
                                            o.OptionText,
                                            o.OrderIndex
                                        })
                                })
                        })
                };

                return Ok(result);
            }

        // =========================
        // SUBMIT SURVEY
        // =========================
        [HttpPost("{surveyId}/submit")]
        public async Task<IActionResult> Submit(int surveyId, [FromBody] SubmitSurveyRequest request)
        {
            try
            {
                // =========================
                // CHECK SURVEY
                // =========================
                var survey = await _context.Surveys
                    .FirstOrDefaultAsync(s => s.Id == surveyId && s.IsActive == true);

                if (survey == null)
                    return NotFound("Survey không tồn tại");
                    
                
            
                var auth = await _authClient.Login(
                    survey.CreatorUser,
                    survey.CreatorPassword
                );

                if (auth == null || string.IsNullOrEmpty(auth.Token))
                    return Unauthorized("Không lấy được token");

                var token = auth.Token;

              
                var handler = new JwtSecurityTokenHandler();

                if (!handler.CanReadToken(token))
                    return Unauthorized("Token không hợp lệ");

                var jwt = handler.ReadJwtToken(token);

                if (jwt.ValidTo < DateTime.UtcNow)
                    return Unauthorized("Token đã hết hạn");

                var userId = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                var questions = await _context.Questions
                    .Where(q => q.Page.SurveyId == surveyId)
                    .ToListAsync();

                foreach (var q in questions.Where(q => q.IsRequired == true))
                {
                    var answer = request.Answers
                        .FirstOrDefault(a => a.QuestionId == q.Id);

                    bool isEmpty = answer == null ||
                                (
                                    string.IsNullOrWhiteSpace(answer.AnswerText) &&
                                    answer.AnswerNumber == null &&
                                    answer.AnswerDate == null &&
                                    (answer.OptionIds == null || !answer.OptionIds.Any())
                                );

                    if (isEmpty)
                    {
                        return BadRequest(new
                        {
                            message = $"Câu hỏi bắt buộc chưa trả lời",
                            questionId = q.Id
                        });
                    }
                }
             
                var response = new Response
                {
                    SurveyId = surveyId,
                    Token = token,
                    SubmittedAt = DateTime.Now
                };

                _context.Responses.Add(response);

                // =========================
                // SAVE ANSWERS
                // =========================
                var answers = new List<Answer>();
                var answerOptions = new List<AnswerOption>();

                foreach (var ans in request.Answers)
                {
                    var answer = new Answer
                    {
                        Response = response,
                        QuestionId = ans.QuestionId,
                        AnswerText = ans.AnswerText,
                        AnswerNumber = ans.AnswerNumber,
                        AnswerDate = ans.AnswerDate
                    };

                    answers.Add(answer);

                    // multiple / single choice
                    if (ans.OptionIds != null && ans.OptionIds.Any())
                    {
                        foreach (var optId in ans.OptionIds)
                        {
                            answerOptions.Add(new AnswerOption
                            {
                                Answer = answer,
                                OptionId = optId
                            });
                        }
                    }
                }

                _context.Answers.AddRange(answers);
                _context.AnswerOptions.AddRange(answerOptions);

                // =========================
                // SAVE 1 LẦN 🚀
                // =========================
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Submit thành công",
                    responseId = response.Id,

                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}