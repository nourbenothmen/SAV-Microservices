using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. Charger Ocelot.json
// ============================================
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddSwaggerGen();

// ============================================
// 2. Config JWT
// ============================================
var jwtSettings = builder.Configuration.GetSection("JWT");

var secretKey = jwtSettings["Key"]
    ?? throw new InvalidOperationException("JWT Key non trouvée dans appsettings.json");

builder.Services
    .AddAuthentication()
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey)
            ),
            ClockSkew = TimeSpan.Zero
        };
    });


// ============================================
// 3. CORS pour Angular
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",
            "https://localhost:4200"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// ============================================
// 4. Ocelot
// ============================================
builder.Services.AddOcelot();

// ============================================
// 5. Logging
// ============================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// ============================================
// 6. Pipeline
// ============================================
app.UseCors("AllowAngular");
// ⚠️ Ajouter ces lignes pour JWT
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    service = "API Gateway",
    status = "Running",
    version = "1.0.0",
    timestamp = DateTime.UtcNow
}));

app.MapGet("/gateway/health", () => Results.Ok(new
{
    status = "Healthy",
    gateway = "Ocelot",
    timestamp = DateTime.UtcNow
}));

await app.UseOcelot();

app.Logger.LogInformation("🚀 API Gateway is running");
app.Run();
