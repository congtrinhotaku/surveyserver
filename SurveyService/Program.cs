using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using SurveyService.Services;
using SurveyService.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ======================
// Add services
// ======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<AuthClient>();
// ======================
// Database
// ======================
builder.Services.AddDbContext<SurveyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ======================
// CORS
// ======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ======================
// JWT Authentication
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
// Authorization (permission lowercase)
// ======================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("survey_view",
        policy => policy.RequireClaim("permission", "survey_view"));

    options.AddPolicy("survey_create",
        policy => policy.RequireClaim("permission", "survey_create"));

    options.AddPolicy("survey_update",
        policy => policy.RequireClaim("permission", "survey_update"));

    options.AddPolicy("survey_delete",
        policy => policy.RequireClaim("permission", "survey_delete"));

    options.AddPolicy("survey_submit",
        policy => policy.RequireClaim("permission", "survey_submit"));
});

// ======================
// Build app
// ======================
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