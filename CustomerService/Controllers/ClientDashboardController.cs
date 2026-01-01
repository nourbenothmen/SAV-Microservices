using CustomerService.Data;
using CustomerService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace CustomerService.Controllers
{
    [ApiController]
    [Route("api/dashboard/client")]
    public class ClientDashboardController : ControllerBase
    {
        private readonly CustomerDbContext _context;
        private readonly HttpClient _httpClient;

        public ClientDashboardController(CustomerDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet("stats")]
        [Authorize]
        public async Task<IActionResult> GetClientStats()
        {
            var userIdClaim = User.FindFirst("uid");
            if (userIdClaim == null) return Unauthorized();

            var userId = userIdClaim.Value;

            var customerId = await _context.Customers
                .Where(c => c.UserId == userId)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (customerId == 0) return NotFound("Client introuvable");

            var totalReclamations = await _context.Reclamations.CountAsync(r => r.CustomerId == customerId);
            var enCours = await _context.Reclamations.CountAsync(r => r.CustomerId == customerId &&
                (r.Status == ReclamationStatus.EnCours || r.Status == ReclamationStatus.Planifiée));
            var terminees = await _context.Reclamations.CountAsync(r => r.CustomerId == customerId && r.Status == ReclamationStatus.Terminée);

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
                    HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", ""));

            var articlesResponse = await _httpClient.GetAsync("https://localhost:7091/apigateway/articles/my");
            int articlesCount = 0;
            if (articlesResponse.IsSuccessStatusCode)
            {
                var articles = await articlesResponse.Content.ReadFromJsonAsync<List<object>>();
                articlesCount = articles?.Count ?? 0;
            }

            return Ok(new
            {
                totalReclamations,
                enCours,
                terminees,
                articles = articlesCount
            });
        }
        [HttpGet("recent")]
        [Authorize]
        public async Task<IActionResult> GetRecentReclamations()
        {
            var userIdClaim = User.FindFirst("uid");
            if (userIdClaim == null) return Unauthorized();

            var userId = userIdClaim.Value;

            var customerId = await _context.Customers
                .Where(c => c.UserId == userId)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (customerId == 0) return NotFound("Client introuvable");

            var reclamations = await _context.Reclamations
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new
                {
                    r.Id,
                    r.ArticleId,
                    r.Description,
                    r.Status,
                    r.CreatedAt,
                    r.ResolvedAt,
                    r.SerialNumber,
                    r.ProblemType,
                    r.DesiredInterventionDate,
                    r.InterventionId
                })
                .ToListAsync();

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
                    HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", ""));

            var result = new List<object>();

            // Récupération unique de estSousGarantie
            Dictionary<int, bool> garantieMap = new();
            try
            {
                var clientArticlesResponse = await _httpClient.GetAsync("https://localhost:7091/apigateway/articles/my");
                if (clientArticlesResponse.IsSuccessStatusCode)
                {
                    var clientArticles = await clientArticlesResponse.Content.ReadFromJsonAsync<List<JsonElement>>();
                    foreach (var ca in clientArticles ?? Enumerable.Empty<JsonElement>())
                    {
                        var articleId = ca.GetProperty("articleId").GetInt32();
                        var estGarantie = ca.GetProperty("estSousGarantie").GetBoolean();
                        garantieMap[articleId] = estGarantie;
                    }
                }
            }
            catch { }

            foreach (var r in reclamations)
            {
                string articleNom = "Article inconnu";
                try
                {
                    var articleResponse = await _httpClient.GetAsync($"https://localhost:7091/apigateway/articles/{r.ArticleId}");
                    if (articleResponse.IsSuccessStatusCode)
                    {
                        var article = await articleResponse.Content.ReadFromJsonAsync<JsonElement>();
                        var marque = article.TryGetProperty("marque", out var m) ? m.GetString() : "";
                        var nom = article.TryGetProperty("nom", out var n) ? n.GetString() : "";
                        var modele = article.TryGetProperty("modele", out var mo) ? mo.GetString() : "";

                        articleNom = string.Join(" - ", new[] { marque, nom, modele != null ? $"Modèle {modele}" : null }
                            .Where(s => !string.IsNullOrWhiteSpace(s)));

                        if (string.IsNullOrWhiteSpace(articleNom))
                            articleNom = "Article inconnu";
                    }
                }
                catch { }

                bool estSousGarantie = garantieMap.GetValueOrDefault(r.ArticleId, false);

                string technicienNom = "Non assigné";
                decimal? montantTotal = null;

                try
                {
                    var interventionResponse = await _httpClient.GetAsync(
                        $"https://localhost:7091/apigateway/interventions/by-reclamation/{r.Id}"
                    );

                    if (interventionResponse.IsSuccessStatusCode)
                    {
                        var interventions = await interventionResponse.Content
                            .ReadFromJsonAsync<List<JsonElement>>();

                        var intervention = interventions?
                            .OrderByDescending(i => i.GetProperty("DateCreation").GetDateTime())
                            .FirstOrDefault();

                        if (intervention.HasValue)
                        {
                            technicienNom = intervention.Value
                                .GetProperty("TechnicienNom")
                                .GetString() ?? "Non assigné";

                            if (r.Status == ReclamationStatus.Terminée && !estSousGarantie)
                            {
                                montantTotal = intervention.Value
                                    .GetProperty("MontantTotal")
                                    .GetDecimal();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur intervention: {ex.Message}");
                }

                result.Add(new
                {
                    r.Id,
                    ArticleNom = articleNom,
                    r.Description,
                    Status = r.Status.ToString(), // ← Important : convertir enum en string
                    r.CreatedAt,
                    r.ResolvedAt,
                    r.SerialNumber,
                    r.ProblemType,
                    r.DesiredInterventionDate,
                    EstSousGarantie = estSousGarantie,
                    TechnicienNom = technicienNom,
                    MontantTotal = montantTotal
                });
            }

            return Ok(result);
        }
    }
}