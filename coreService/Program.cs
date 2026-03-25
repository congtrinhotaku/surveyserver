using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using coreService.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ======================
// Add services
// ======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SurveyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ======================
// ✅ CORS (THÊM MỚI)
// ======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// ======================
// JWT Config
// ======================
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// ======================
// Authorization
// ======================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("user_view", policy =>
        policy.RequireClaim("permission", "user_view"));
    options.AddPolicy("user_create", policy =>
        policy.RequireClaim("permission", "user_create"));
    options.AddPolicy("user_update", policy =>
        policy.RequireClaim("permission", "user_update"));
    options.AddPolicy("user_delete", policy =>
        policy.RequireClaim("permission", "user_delete"));

    options.AddPolicy("role_view", policy =>
        policy.RequireClaim("permission", "role_view"));
    options.AddPolicy("role_create", policy =>
        policy.RequireClaim("permission", "role_create"));
    options.AddPolicy("role_update", policy =>
        policy.RequireClaim("permission", "role_update"));
    options.AddPolicy("role_delete", policy =>
        policy.RequireClaim("permission", "role_delete"));

    options.AddPolicy("function_view", policy =>
        policy.RequireClaim("permission", "function_view"));
    options.AddPolicy("function_update", policy =>
        policy.RequireClaim("permission", "function_update"));

    options.AddPolicy("menu_view", policy =>
        policy.RequireClaim("permission", "menu_view"));
    options.AddPolicy("menu_create", policy =>
        policy.RequireClaim("permission", "menu_create"));
    options.AddPolicy("menu_update", policy =>
        policy.RequireClaim("permission", "menu_update"));
    options.AddPolicy("menu_delete", policy =>
        policy.RequireClaim("permission", "menu_delete"));
});

var app = builder.Build();

// ======================
// Middleware
// ======================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseCors("AllowAll");


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();