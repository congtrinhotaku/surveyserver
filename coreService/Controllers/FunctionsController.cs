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
    
    public class FunctionsController : ControllerBase
    {
        private readonly SurveyDbContext _context;

        public FunctionsController(SurveyDbContext context)
        {
            _context = context;
        }

        // ======================
        // GET FUNCTION NAMES
        // ======================
        [HttpGet]
        [Authorize(Policy = "function_view")]
        public async Task<IActionResult> GetFunctions()
        {
            var functions = await _context.Functions
                .Select(f => new
                {
                    f.Id,
                    f.Name,
                    f.Code
                })
                .ToListAsync();

            return Ok(functions);
        }

        // ======================
        // GET DETAIL FUNCTION
        // ======================
       [HttpGet("{functionId}")]
       [Authorize(Policy = "function_view")]
        public async Task<IActionResult> GetFunctionDetail(int functionId)
        {
            var function = await _context.Functions
                .Include(f => f.Permissions)
                .FirstOrDefaultAsync(f => f.Id == functionId);

            if (function == null)
                return NotFound("Function không tồn tại");

            var roles = await _context.Roles
                .Include(r => r.Permissions)
                .ToListAsync();

            var result = roles.Select(role => new
            {
                FunctionID = function.Id,
                FunctionName = function.Name,
                RoleID = role.Id,
                RoleName = role.Name,

                Permissions = function.Permissions.Select(p => new
                {
                    PermissionID = p.Id,
                    Action = p.Code.Split('_')[1], // view, create...
                    IsActive = role.Permissions.Any(rp => rp.Id == p.Id)
                })
            });

            return Ok(result);
        }

        // ======================
        // UPDATE PERMISSION ROLE
        // ======================
        [HttpPut("update-permission")]
        [Authorize(Policy = "function_update")]
        public async Task<IActionResult> UpdatePermission([FromBody] UpdatePermissionRequest request)
        {
            // 1. Lấy role + permissions
            var role = await _context.Roles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == request.RoleId);

            if (role == null)
                return NotFound("Role không tồn tại");

            // 2. Lấy permission
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == request.PermissionId);

            if (permission == null)
                return NotFound("Permission không tồn tại");

            // 3. Check đã tồn tại chưa
            var exists = role.Permissions.Any(p => p.Id == request.PermissionId);

            // 4. Xử lý add/remove
            if (request.IsActive)
            {
                if (!exists)
                {
                    role.Permissions.Add(permission);
                }
            }
            else
            {
                if (exists)
                {
                    role.Permissions.Remove(permission);
                }
            }

            // 5. Save
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Cập nhật thành công",
                RoleId = request.RoleId,
                PermissionId = request.PermissionId,
                IsActive = request.IsActive
            });
        }
    }
    // ======================
    // DTO
    // ======================

}