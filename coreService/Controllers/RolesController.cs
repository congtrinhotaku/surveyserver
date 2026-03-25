using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using coreService.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
namespace coreService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly SurveyDbContext _context;

        public RolesController(SurveyDbContext context)
        {
            _context = context;
        }

        // ======================
        // GET: api/roles
        // ======================
        [HttpGet]
        [Authorize(Policy = "role_view")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    Permissions = r.Permissions.Select(p => p.Code).ToList()
                })
                .ToListAsync();

            return Ok(roles);
        }

        // ======================
        // GET: api/roles/{id}
        // ======================
        [HttpGet("{id}")]
        [Authorize(Policy = "role_view")]
        public async Task<IActionResult> GetRole(int id)
        {
            var role = await _context.Roles
                .Include(r => r.Permissions)
                .Where(r => r.Id == id)
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    Permissions = r.Permissions.Select(p => new { p.Id, p.Code, p.Name }).ToList()
                })
                .FirstOrDefaultAsync();

            if (role == null) return NotFound("Role không tồn tại");

            return Ok(role);
        }

        // ======================
        // POST: api/roles
        // ======================
        [HttpPost]
        [Authorize(Policy = "role_create")]
        public async Task<IActionResult> CreateRole([FromBody] RoleNameRequest request)
        {
            if (await _context.Roles.AnyAsync(r => r.Name == request.Name))
                return BadRequest("Tên role đã tồn tại");

            var role = new Role
            {
                Name = request.Name
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return Ok(role);
        }

        // ======================
        // PUT: api/roles/{id}
        // ======================
        [HttpPut("{id}")]
        [Authorize(Policy = "role_update")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleNameRequest request)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();

            if (await _context.Roles.AnyAsync(r => r.Name == request.Name && r.Id != id))
                return BadRequest("Tên role đã tồn tại");

            role.Name = request.Name;

            await _context.SaveChangesAsync();
            return Ok(role);
        }
        // ======================
        // DELETE: api/roles/{id}
        // ======================
        [HttpDelete("{id}")]
        [Authorize(Policy = "role_delete")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound("Role không tồn tại");

            // Lưu ý: Cần xử lý User đang dùng Role này nếu cần thiết
            // EF Core sẽ tự xóa bảng trung gian RolePermissions, nhưng UserRoles cần xem xét Constraint
            
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return Ok($"Đã xóa role {id}");
        }
    }

    public class CreateRoleModel
    {
        public string Name { get; set; } = null!;
        public List<int>? PermissionIds { get; set; }
    }
}