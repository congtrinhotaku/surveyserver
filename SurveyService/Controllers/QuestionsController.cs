using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurveyService.Models;

namespace SurveyService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly SurveyDbContext _context;

        public QuestionsController(SurveyDbContext context)
        {
            _context = context;
        }

        // =========================
        // CREATE QUESTION
        // =========================
        [HttpPost]
        [Authorize(Policy = "survey_update")]
        public async Task<IActionResult> Create([FromBody] CreateQuestionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var page = await _context.Pages.FindAsync(request.PageId);
            if (page == null)
                return NotFound("Page không tồn tại");

            var question = new Question
            {
                PageId = request.PageId,
                QuestionText = request.QuestionText,
                QuestionTypeId = request.QuestionTypeId,
                IsRequired = request.IsRequired,
                OrderIndex = request.OrderIndex
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            // chỉ thêm option nếu là choice
            var hasOptions = request.QuestionTypeId == 1 || request.QuestionTypeId == 2;

            if (hasOptions && request.Options != null && request.Options.Any())
            {
                var options = request.Options.Select((opt, index) => new Option
                {
                    QuestionId = question.Id,
                    OptionText = opt,
                    OrderIndex = index + 1
                });

                _context.Options.AddRange(options);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                question.Id,
                question.QuestionText,
                question.QuestionTypeId,
                question.IsRequired,
                question.OrderIndex,
                Options = new List<object>()
            });
        }

        // =========================
        // GET QUESTIONS BY PAGE
        // =========================
        [HttpGet("page/{pageId}")]
        [Authorize(Policy = "survey_view")]
        public async Task<IActionResult> GetByPage(int pageId)
        {
            var questions = await _context.Questions
                .Where(q => q.PageId == pageId)
                .Include(q => q.Options)
                .OrderBy(q => q.OrderIndex)
                .ToListAsync();

            var result = questions.Select(q => new
            {
                q.Id,
                q.QuestionText,
                q.QuestionTypeId,
                q.IsRequired,
                q.OrderIndex,
                Options = q.Options.Select(o => new
                {
                    o.Id,
                    o.OptionText,
                    o.OrderIndex
                })
            });

            return Ok(result);
        }

        // =========================
        // UPDATE QUESTION
        // =========================
        [HttpPut("{id}")]
        [Authorize(Policy = "survey_update")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateQuestionRequest request)
        {
            try
            {
                Console.WriteLine("=== UPDATE QUESTION ===");

                var question = await _context.Questions
                    .Include(q => q.Options)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (question == null)
                    return NotFound("Question không tồn tại");

                // ======================
                // UPDATE INFO
                // ======================
                question.QuestionText = request.QuestionText;
                question.QuestionTypeId = request.QuestionTypeId;
                question.IsRequired = request.IsRequired;
                question.OrderIndex = request.OrderIndex;

                var hasOptions = request.QuestionTypeId == 1 || request.QuestionTypeId == 2;

                // ======================
                // HANDLE OPTIONS
                // ======================
                if (hasOptions && request.Options != null)
                {
                    var existingOptions = question.Options
                        .OrderBy(o => o.OrderIndex)
                        .ToList();

                    int newCount = request.Options.Count;
                    int oldCount = existingOptions.Count;

                    // ======================
                    // 1. UPDATE & ADD
                    // ======================
                    for (int i = 0; i < newCount; i++)
                    {
                        var newOpt = request.Options[i];

                        if (string.IsNullOrWhiteSpace(newOpt.OptionText))
                            continue;

                        if (i < oldCount)
                        {
                            // 👉 UPDATE option cũ
                            existingOptions[i].OptionText = newOpt.OptionText;
                            existingOptions[i].OrderIndex = i + 1;
                        }
                        else
                        {
                            // 👉 ADD option mới
                            _context.Options.Add(new Option
                            {
                                QuestionId = id,
                                OptionText = newOpt.OptionText,
                                OrderIndex = i + 1
                            });
                        }
                    }

                    // ======================
                    // 2. REMOVE dư (nếu có)
                    // ======================
                    if (oldCount > newCount)
                    {
                        var extraOptions = existingOptions
                            .Skip(newCount)
                            .ToList();

                        foreach (var opt in extraOptions)
                        {
                            // ❗ check đã có người chọn chưa
                            bool isUsed = await _context.AnswerOptions
                                .AnyAsync(a => a.OptionId == opt.Id);

                            if (!isUsed)
                            {
                                _context.Options.Remove(opt);
                            }
                            else
                            {
                                // 👉 KHÔNG xóa → chỉ disable hoặc giữ nguyên
                                Console.WriteLine($"⚠ Option {opt.Id} đang được dùng, không xóa");

                                // optional: đánh dấu
                                opt.OptionText += " (đã dùng)";
                            }
                        }
                    }
                }
                else
                {
                    // ======================
                    // Nếu đổi sang text (không cần options)
                    // ======================
                    var existingOptions = await _context.Options
                        .Where(o => o.QuestionId == id)
                        .ToListAsync();

                    foreach (var opt in existingOptions)
                    {
                        bool isUsed = await _context.AnswerOptions
                            .AnyAsync(a => a.OptionId == opt.Id);

                        if (!isUsed)
                        {
                            _context.Options.Remove(opt);
                        }
                        else
                        {
                            Console.WriteLine($"⚠ Option {opt.Id} đang được dùng, giữ lại");
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Update thành công",
                    question.Id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ERROR UPDATE: " + ex.ToString());
                return StatusCode(500, ex.Message);
            }
        }

        // =========================
        // DELETE QUESTION
        // =========================
        [HttpDelete("{id}")]
        [Authorize(Policy = "survey_update")]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
                return NotFound();

            var options = await _context.Options
                .Where(o => o.QuestionId == id)
                .ToListAsync();

            _context.Options.RemoveRange(options);
            _context.Questions.Remove(question);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa câu hỏi" });
        }
    }
}