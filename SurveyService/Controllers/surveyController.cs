using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurveyService.Models;

namespace SurveyService.Controllers
{
    [Route("api/")]
    [ApiController]
    public class SurveyController : ControllerBase
    {
        private readonly SurveyDbContext _context;

        public SurveyController(SurveyDbContext context)
        {
            _context = context;
        }



        // =========================
        // CREATE SURVEY
        // =========================
        [HttpPost("create")]
        [Authorize(Policy = "survey_create")]
        public async Task<IActionResult> CreateSurvey([FromBody] CreateSurveyRequest request)
        {

            var survey = new Survey
            {
                Title = request.Title,
                Description = request.Description,
                CreatorUser = request.Username,
                CreatorPassword = request.Password
            };

            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();

            return Ok(survey);
        }

        // =========================
        // GET ALL SURVEYS
        // =========================
        [HttpGet]
        [Authorize(Policy = "survey_view")]
        public async Task<IActionResult> GetAll()
        {
            var surveys = await _context.Surveys
                .ToListAsync();

            
            return Ok(surveys);
        }

        // =========================
        // GET BY ID
        // =========================
        [HttpGet("{id}")]
        [Authorize(Policy = "survey_view")]
          public async Task<IActionResult> GetById(int id)
            {
                var survey = await _context.Surveys
                    .Include(x => x.Pages)
                        .ThenInclude(p => p.Questions)
                            .ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(x => x.Id == id);

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
        // UPDATE SURVEY
        // =========================
        [HttpPut("{id}")]
        [Authorize(Policy = "survey_update")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSurveyRequest request)
        {


            var survey = await _context.Surveys.FindAsync(id);
            if (survey == null)
                return NotFound();



            survey.Title = request.Title;
            survey.Description = request.Description;

            await _context.SaveChangesAsync();

            return Ok(survey);
        }

        // =========================
        // DELETE SURVEY
        // =========================
        [HttpDelete("{id}")]
        [Authorize(Policy = "survey_delete")]
        public async Task<IActionResult> Delete(int id)
        {


            var survey = await _context.Surveys.FindAsync(id);
            if (survey == null)
                return NotFound();


            _context.Surveys.Remove(survey);
            await _context.SaveChangesAsync();

            return Ok("Đã xóa");
        }
    }

   



}