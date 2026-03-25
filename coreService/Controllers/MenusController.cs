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
    public class MenusController : ControllerBase
    {
        private readonly SurveyDbContext _context;

        public MenusController(SurveyDbContext context)
        {
            _context = context;
        }

        // ======================
        // GET ALL MENU (FLAT)
        // ======================
        [HttpGet]
        [Authorize(Policy = "menu_view")]
        public async Task<IActionResult> GetMenus()
        {
            var menus = await _context.Menus
                .OrderBy(m => m.OrderIndex)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.Path,
                    m.ParentId,
                    m.Icon,
                    m.OrderIndex,
                    m.CreatedAt
                })
                .ToListAsync();

            return Ok(menus);
        }

        // ======================
        // GET MENU TREE (QUAN TRỌNG)
        // ======================
        [HttpGet("tree")]
        [Authorize(Policy = "menu_view")]
        public async Task<IActionResult> GetMenuTree()
        {
            var menus = await _context.Menus
                .OrderBy(m => m.OrderIndex)
                .ToListAsync();

            var result = BuildTree(menus, null);

            return Ok(result);
        }


        // ======================
        // GET MY MENU
        // ======================
        [HttpGet("my-menu")]
        [Authorize]
        public async Task<IActionResult> GetMyMenu()
        {
            // ======================
            // 1. Lấy thông tin từ token
            // ======================
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            if (userId == null && username == null)
                return Unauthorized("Token không hợp lệ");

            // ======================
            // 2. Lấy user + roles + permissions
            // ======================
            var userQuery = _context.Users
                .Include(u => u.Roles)
                    .ThenInclude(r => r.Permissions)
                .AsQueryable();

            var user = userId != null
                ? await userQuery.FirstOrDefaultAsync(u => u.Id.ToString() == userId)
                : await userQuery.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return NotFound("User không tồn tại");

            // ======================
            // 3. Lấy danh sách permission
            // ======================
            var permissionCodes = user.Roles
                .SelectMany(r => r.Permissions)
                .Select(p => p.Code) // ví dụ: menu_view, user_create
                .Distinct()
                .ToList();

            // ======================
            // 4. Lấy function theo permission
            // ======================
            var functions = await _context.Functions
                .Include(f => f.Permissions)
                .Where(f => f.Permissions.Any(p => permissionCodes.Contains(p.Code)))
                .ToListAsync();

            var functionIds = functions.Select(f => f.Id).ToList();

            // ======================
            // 5. Lấy menu theo function
            // ======================
            var menus = await _context.Menus
                .Where(m => m.FunctionId == null || functionIds.Contains(m.FunctionId.Value))
                .OrderBy(m => m.OrderIndex)
                .ToListAsync();

            // ======================
            // 6. Build tree menu
            // ======================
            var result = BuildTree(menus, null);

            return Ok(result);
        }

        private List<object> BuildTree(List<Menu> menus, int? parentId)
        {
            return menus
                .Where(m => m.ParentId == parentId)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.Path,
                    m.Icon,
                    m.OrderIndex,
                    Children = BuildTree(menus, m.Id)
                })
                .ToList<object>();
        }

        // ======================
        // GET MENU BY ID
        // ======================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMenu(int id)
        {
            var menu = await _context.Menus.FindAsync(id);

            if (menu == null)
                return NotFound("Menu không tồn tại");

            return Ok(menu);
        }

        // ======================
        // CREATE MENU
        // ======================
        [HttpPost]
        [Authorize(Policy = "menu_create")]
        public async Task<IActionResult> CreateMenu([FromBody] CreateMenuRequest request)
        {
            if (string.IsNullOrEmpty(request.Name))
                return BadRequest("Tên menu không được để trống");

            var menu = new Menu
            {
                Name = request.Name,
                Path = request.Path,
                ParentId = request.ParentId,
                Icon = request.Icon,
                OrderIndex = request.OrderIndex,
                CreatedAt = DateTime.Now
            };

            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Tạo menu thành công", MenuId = menu.Id });
        }

        // ======================
        // UPDATE MENU
        // ======================
        [HttpPut("{id}")]
        [Authorize(Policy = "menu_update")]
        public async Task<IActionResult> UpdateMenu(int id, [FromBody] UpdateMenuRequest request)
        {
            var menu = await _context.Menus.FindAsync(id);

            if (menu == null)
                return NotFound("Menu không tồn tại");

            if (!string.IsNullOrEmpty(request.Name))
                menu.Name = request.Name;

            if (request.Path != null)
                menu.Path = request.Path;

            if (request.Icon != null)
                menu.Icon = request.Icon;

            if (request.ParentId.HasValue)
                menu.ParentId = request.ParentId;

            if (request.OrderIndex.HasValue)
                menu.OrderIndex = request.OrderIndex;

            await _context.SaveChangesAsync();

            return Ok($"Cập nhật menu {id} thành công");
        }

        // ======================
        // DELETE MENU
        // ======================
        [HttpDelete("{id}")]
        [Authorize(Policy = "menu_delete")]
        public async Task<IActionResult> DeleteMenu(int id)
        {
            var menu = await _context.Menus.FindAsync(id);

            if (menu == null)
                return NotFound("Menu không tồn tại");

            // check có con không
            var hasChild = await _context.Menus.AnyAsync(m => m.ParentId == id);
            if (hasChild)
                return BadRequest("Menu đang có menu con, không thể xóa");

            _context.Menus.Remove(menu);
            await _context.SaveChangesAsync();

            return Ok($"Đã xóa menu {id}");
        }
    }


 
}