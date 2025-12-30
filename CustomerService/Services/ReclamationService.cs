// Services/ReclamationService.cs
using CustomerService.Data;
using CustomerService.Models;
using CustomerService.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using CustomerService.Models.DTOs;


namespace CustomerService.Services
{
    public class ReclamationService : IReclamationService
    {
        private readonly CustomerDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ReclamationService> _logger;


        public ReclamationService(CustomerDbContext context, HttpClient httpClient, ILogger<ReclamationService> logger)
        {
            _context = context;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7091/apigateway/");
            _logger = logger;
        }

      public async Task<Reclamation> CreateReclamationAsync(string userId, CreateReclamationDto dto)
{
    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
    if (customer == null)
        throw new KeyNotFoundException("Profil client introuvable.");

    // Récupérer le CustomerArticle pour cet article et ce client
    string serialNumber = "";
    try
    {
        var response = await _httpClient.GetAsync($"articles/customer-articles/client/{customer.Id}/article/{dto.ArticleId}");
        if (response.IsSuccessStatusCode)
        {
            var caList = await response.Content.ReadFromJsonAsync<List<dynamic>>();
            var ca = caList?.FirstOrDefault();
            if (ca != null)
            {
                serialNumber = ca.NumeroSerie ?? "";
            }
        }
    }
    catch { }

    var reclamation = new Reclamation
    {
        CustomerId = customer.Id,
        ArticleId = dto.ArticleId,
        SerialNumber = serialNumber, // ← ENREGISTRÉ !
        Description = dto.Description +
            (string.IsNullOrEmpty(dto.ProblemType) ? "" : $"\nType de problème : {dto.ProblemType}") +
            (dto.DesiredInterventionDate.HasValue ? $"\nDate souhaitée : {dto.DesiredInterventionDate:dd/MM/yyyy}" : ""),
        ProblemType = dto.ProblemType,
        DesiredInterventionDate = dto.DesiredInterventionDate,
        Status = ReclamationStatus.EnAttente,
        CreatedAt = DateTime.UtcNow
    };

    _context.Reclamations.Add(reclamation);
    await _context.SaveChangesAsync();
    return reclamation;
}
        public async Task<List<ReclamationDto>> GetMyReclamationsAsync(string userId)
        {
            var customer = await _context.Customers
                .Include(c => c.Reclamations)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
                return new List<ReclamationDto>();

            var dtos = new List<ReclamationDto>();

            foreach (var r in customer.Reclamations)
            {
                string articleNom = "Article inconnu";
                bool estSousGarantie = false;

                try
                {
                    var response = await _httpClient.GetAsync($"articles/{r.ArticleId}");
                    if (response.IsSuccessStatusCode)
                    {
                        var article = await response.Content.ReadFromJsonAsync<JsonElement>();
                        articleNom =
                            $"{article.GetProperty("marque").GetString()} - " +
                            $"{article.GetProperty("nom").GetString()} - " +
                            $"Modèle {article.GetProperty("modele").GetString()}";

                        // Optionnel : récupérer garantie
                        if (article.TryGetProperty("articlesClients", out var clients) &&
                            clients.ValueKind == JsonValueKind.Array &&
                            clients.GetArrayLength() > 0)
                        {
                            estSousGarantie = clients[0].GetProperty("estSousGarantie").GetBoolean();
                        }
                    }
                }
                catch { /* logger si besoin */ }

                dtos.Add(new ReclamationDto
                {
                    Id = r.Id,
                    ArticleId = r.ArticleId,
                    ArticleNom = articleNom,
                    Description = r.Description,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    ResolvedAt = r.ResolvedAt,
                    ProblemType = r.ProblemType,
                    DesiredInterventionDate = r.DesiredInterventionDate,
                    EstSousGarantie = estSousGarantie,
                    CustomerId = r.CustomerId,
                    CustomerNom = $"{customer.FirstName} {customer.LastName}"
                });
            }

            return dtos;
        }

        public async Task<List<ReclamationDto>> GetAllReclamationsAsync()
        {
            var reclamations = await _context.Reclamations
                .Include(r => r.Customer) // 🔹 inclure le client
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var dtos = new List<ReclamationDto>();

            foreach (var r in reclamations)
            {
                string articleNom = "Article inconnu";
                bool estSousGarantie = false;

                try
                {
                    var response = await _httpClient.GetAsync($"articles/{r.ArticleId}");
                    if (response.IsSuccessStatusCode)
                    {
                        var article = await response.Content.ReadFromJsonAsync<JsonElement>();
                        articleNom =
                            $"{article.GetProperty("marque").GetString()} - " +
                            $"{article.GetProperty("nom").GetString()} - " +
                            $"Modèle {article.GetProperty("modele").GetString()}";

                        if (article.TryGetProperty("articlesClients", out var clients) &&
                            clients.ValueKind == JsonValueKind.Array &&
                            clients.GetArrayLength() > 0)
                        {
                            estSousGarantie = clients[0].GetProperty("estSousGarantie").GetBoolean();
                        }
                    }
                }
                catch { /* logger */ }

                dtos.Add(new ReclamationDto
                {
                    Id = r.Id,
                    ArticleId = r.ArticleId,
                    ArticleNom = articleNom,
                    Description = r.Description,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    ResolvedAt = r.ResolvedAt,
                    ProblemType = r.ProblemType,
                    DesiredInterventionDate = r.DesiredInterventionDate,
                    EstSousGarantie = estSousGarantie,
                    CustomerId = r.CustomerId,
                    CustomerNom = $"{r.Customer?.FirstName} {r.Customer?.LastName}" ?? "N/A"
                });
            }

            return dtos;
        }

        public async Task<Reclamation?> GetReclamationByIdAsync(int reclamationId)
        {
            return await _context.Reclamations
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == reclamationId);
        }

        public async Task<bool> UpdateReclamationStatusAsync(int reclamationId, ReclamationStatus status)
        {
            var reclamation = await _context.Reclamations.FindAsync(reclamationId);
            if (reclamation == null) return false;

            reclamation.Status = status;
            if (status == ReclamationStatus.Terminée)
                reclamation.ResolvedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Reclamation> UpdateReclamationAsync(int id, Reclamation updatedReclamation)
        {
            var existing = await _context.Reclamations.FindAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Réclamation {id} non trouvée");

            // Mise à jour des champs modifiables
            existing.Status = updatedReclamation.Status;
            existing.ProcessedAt = DateTime.UtcNow; // ou une autre date de traitement
                                                    // Ajoute d'autres champs si besoin (ex: ResolvedAt si terminée)

            _context.Reclamations.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }

    }
}