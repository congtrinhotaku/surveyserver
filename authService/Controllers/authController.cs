using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using authService.Models;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SurveyDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(SurveyDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Validate input
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Thiếu username hoặc password");

            // 2. Tìm user
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Username == request.Username && x.IsActive == true);

            if (user == null)
                return Unauthorized("User không tồn tại");

            // 3. Check password
            bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isValid)
                return Unauthorized("Sai mật khẩu");

            // 4. Lấy roles
            var roles = await _context.Users
                .Where(u => u.Id == user.Id)
                .SelectMany(u => u.Roles.Select(r => r.Name))
                .ToListAsync();

            
            var permissions = await _context.Users
                .Where(u => u.Id == user.Id)
                .SelectMany(u => u.Roles
                    .SelectMany(r => r.Permissions
                        .Select(p => p.Code)))
                .Distinct()
                .ToListAsync();

            // 6. Tạo token
            var token = GenerateJwtToken(user, roles, permissions);
            Console.WriteLine($"Generated JWT: {string.Join(", ", permissions)}");
            return Ok(new
            {
                token,
                id = user.Id,
                username = user.Username,
                roles,
                permissions
            });
        }

        private string GenerateJwtToken(User user, List<string> roles, List<string> permissions)
        {
            // check key null
            var jwtKey = _config["Jwt:Key"] ?? throw new Exception("Missing Jwt:Key");
            var key = Encoding.ASCII.GetBytes(jwtKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("id", user.Id.ToString())
            };

            // add roles
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // add permissions
            foreach (var p in permissions)
            {
                claims.Add(new Claim("permission", p));
            }

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}