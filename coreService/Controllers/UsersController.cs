using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using coreService.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace coreService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SurveyDbContext _context;

        public UsersController(SurveyDbContext context)
        {
            _context = context;
        }

        // ======================
        // VIEW USERS
        // ======================
        [HttpGet]
        [Authorize(Policy = "user_view")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.IsActive,
                    Roles = u.Roles.Select(r => r.Name).ToList()
                })
                .ToListAsync();

            return Ok(users);
        }

        // ======================
        // CREATE USER
        // ======================
        [HttpPost]
        [Authorize(Policy = "user_create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Vui lòng nhập Username và Password");

            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Username đã tồn tại");

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                IsActive = true
            };

            // Gán Role nếu có
            if (request.RoleIds != null && request.RoleIds.Any())
            {
                var roles = await _context.Roles
                    .Where(r => request.RoleIds.Contains(r.Id))
                    .ToListAsync();
                
                foreach (var role in roles)
                {
                    newUser.Roles.Add(role);
                }
            }

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Tạo user thành công", UserId = newUser.Id });
        }

        // ======================
        // UPDATE USER
        // ======================
        [HttpPut("{id}")]
        [Authorize(Policy = "user_update")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound("User không tồn tại");

            // Update trạng thái
            if (request.IsActive.HasValue)
                user.IsActive = request.IsActive.Value;

            // Reset mật khẩu nếu có gửi lên
            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            // Update Roles (Ghi đè)
            if (request.RoleIds != null)
            {
                user.Roles.Clear();
                var roles = await _context.Roles
                    .Where(r => request.RoleIds.Contains(r.Id))
                    .ToListAsync();

                foreach (var role in roles)
                {
                    user.Roles.Add(role);
                }
            }

            await _context.SaveChangesAsync();
            return Ok($"Cập nhật user {id} thành công");
        }

        // ======================
        // DELETE USER
        // ======================
        [HttpDelete("{id}")]
        [Authorize(Policy = "user_delete")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) 
                return NotFound("User không tồn tại");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok($"Đã xóa user {id}");
        }
    }

    // DTOs


  
}