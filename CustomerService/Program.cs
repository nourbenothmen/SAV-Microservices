// Program.cs - CustomerService (CORRIGÉ selon appsettings.json)
using CustomerService.Data;
using CustomerService.Services;
using CustomerService.Services.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. Configuration de la base de données
// ============================================
builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CustomerDb")
        ?? throw new InvalidOperationException("Connection string 'CustomerDb' not found.")));

// ============================================
// 2. Configuration HttpClients pour appeler les autres services
// ============================================
// HttpClient pour AuthService
builder.Services.AddHttpClient<AuthApiClient>(client =>
{
    // Utiliser HTTPS si AuthService l'écoute
    client.BaseAddress = new Uri("https://localhost:7163");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// HttpClient pour ArticleService
builder.Services.AddHttpClient("ArticleService", client =>
{
    var articleServiceUrl = builder.Configuration["Services:ArticleService"];
    client.BaseAddress = new Uri(articleServiceUrl ?? "https://localhost:7087");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// HttpClient générique
builder.Services.AddHttpClient();

// ============================================
// 3. Enregistrement des services
// ============================================
builder.Services.AddScoped<ICustomerService, CustomerService.Services.CustomerService>();
builder.Services.AddScoped<IReclamationService, ReclamationService>();

// ============================================
// 4. Configuration JWT Authentication
// ============================================
// ✅ CORRECTION: Utiliser "Key" au lieu de "SecretKey"
var jwtSettings = builder.Configuration.GetSection("JWT");
var jwtKey = jwtSettings["Key"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey!)
            ),
            // ⭐⭐ LIGNE CRITIQUE ⭐⭐
            RoleClaimType = ClaimTypes.Role,

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();


// ============================================
// 5. Configuration CORS
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ============================================
// 6. Configuration Controllers + Swagger
// ============================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

        // ⭐⭐ IMPORTANT : Enum → string ⭐⭐
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });


builder.Services.AddEndpointsApiExplorer();

// Configuration de Swagger/OpenAPI avec JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Customer Service API",
        Version = "v1",
        Description = "API pour la gestion des articles sanitaires et de chauffage"
    });

    // Configuration pour l'authentification JWT dans Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header en utilisant le sch�ma Bearer. Entrez 'Bearer' [espace] puis votre token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// ============================================
// 7. Health Checks
// ============================================


// ============================================
// 8. Logging
// ============================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ============================================
// 9. Construction de l'application
// ============================================
var app = builder.Build();

// ============================================
// 10. Configuration du Pipeline Middleware
// ============================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CustomerService V1");
        c.RoutePrefix = "swagger"; // 🔥
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
//app.MapHealthChecks("/health");

// Endpoint racine
app.MapGet("/", () => Results.Ok(new
{
    service = "CustomerService",
    status = "Running",
    version = "1.0.0",
    timestamp = DateTime.UtcNow
}));

// ============================================
// 11. Auto-migration de la base de données
// ============================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<CustomerDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Application des migrations de base de données...");
        db.Database.Migrate();
        logger.LogInformation("Migrations appliquées avec succès");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erreur lors de l'initialisation de la base de données");
    }
}

app.Logger.LogInformation("🚀 CustomerService démarré avec succès sur {Environment}", app.Environment.EnvironmentName);

app.Run();