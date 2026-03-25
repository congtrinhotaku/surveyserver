using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurveyService.Models;

namespace SurveyService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagesController : ControllerBase
    {
        private readonly SurveyDbContext _context;

        public PagesController(SurveyDbContext context)
        {
            _context = context;
        }

        // =========================
        // CREATE PAGE
        // =========================
        [HttpPost]
        [Authorize(Policy = "survey_update")]
       public async Task<IActionResult> Create(CreatePageRequest request)
        {
            var survey = await _context.Surveys.FindAsync(request.SurveyId);
            if (survey == null) return NotFound();

            var page = new Page
            {
                SurveyId = request.SurveyId,
                Title = request.Title,
                OrderIndex = request.OrderIndex
            };

            _context.Pages.Add(page);
            await _context.SaveChangesAsync();

    
            return Ok(new
            {
                page.Id,
                page.Title,
                page.OrderIndex,
                page.SurveyId
            });
        }

        // =========================
        // UPDATE PAGE
        // =========================
        [HttpPut("{id}")]
        [Authorize(Policy = "survey_update")]
        public async Task<IActionResult> Update(int id, UpdatePageRequest request)
        {
            var page = await _context.Pages.FindAsync(id);
            if (page == null) return NotFound();

            page.Title = request.Title;
            page.OrderIndex = request.OrderIndex;

            await _context.SaveChangesAsync();

            return Ok(page);
        }

        // =========================
        // DELETE PAGE
        // =========================
        [HttpDelete("{id}")]
        [Authorize(Policy = "survey_update")]
        public async Task<IActionResult> Delete(int id)
        {
            var page = await _context.Pages
                .Include(p => p.Questions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null) return NotFound();

            _context.Questions.RemoveRange(page.Questions);
            _context.Pages.Remove(page);

            await _context.SaveChangesAsync();

            return Ok("Đã xóa page");
        }
    }
}