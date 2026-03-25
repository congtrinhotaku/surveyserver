using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ======================
// Load Ocelot config
// ======================
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// ======================
// CORS (🔥 QUAN TRỌNG)
// ======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// ======================
// JWT (để gateway hiểu token)
// ======================
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

// ======================
// Ocelot
// ======================
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// ======================
// MIDDLEWARE (THỨ TỰ RẤT QUAN TRỌNG)
// ======================

// 🔥 CORS phải đứng đầu
app.UseCors("AllowFrontend");

// 🔥 Fix preflight OPTIONS
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 200;
        return;
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// 👉 Ocelot luôn cuối
await app.UseOcelot();

app.Run();