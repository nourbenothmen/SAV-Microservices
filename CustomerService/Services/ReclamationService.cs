// Services/ReclamationService.cs
using CustomerService.Data;
using CustomerService.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Services
{
    public class ReclamationService : IReclamationService
    {
        private readonly CustomerDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;


        public ReclamationService(CustomerDbContext context, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<Reclamation> CreateReclamationAsync(string userId, CreateReclamationDto dto)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId.ToString());


            if (customer == null)
                throw new KeyNotFoundException("Profil client introuvable. Veuillez compléter votre profil.");

            // Vérifier que l'article existe (appel ArticleService)

            //var articleResponse = await _httpClient.GetAsync($"{_config["Services:ArticleService"]}/api/articles/{dto.ArticleId}");
            //var articleResponse = await _httpClient.GetAsync($"{_config["Services:ArticleService"]}/{dto.ArticleId}");
            //var articleResponse = await _httpClient.GetAsync($"{_config["Services:ArticleService"]}/{dto.ArticleId}");
            var client = _httpClientFactory.CreateClient("ArticleService"); // Nom du client configuré dans Program.cs
            var articleResponse = await client.GetAsync($"/api/Articles/{dto.ArticleId}");

            if (!articleResponse.IsSuccessStatusCode)
                throw new InvalidOperationException("Article introuvable.");

            var reclamation = new Reclamation
            {
                CustomerId = customer.Id,
                ArticleId = dto.ArticleId,
                Description = dto.Description,
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
    .FirstOrDefaultAsync(c => c.UserId == userId.ToString());


            if (customer == null) return new List<ReclamationDto>();

            return customer.Reclamations.Select(MapToDto).ToList();
        }

        public async Task<List<ReclamationDto>> GetAllReclamationsAsync()
        {
            var reclamations = await _context.Reclamations
                .Include(r => r.Customer)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reclamations.Select(MapToDto).ToList();
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
            if (status == ReclamationStatus.Resolue)
                reclamation.ResolvedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private static ReclamationDto MapToDto(Reclamation r) => new()
        {
            Id = r.Id,
            ArticleId = r.ArticleId,
            Description = r.Description,
            Status = r.Status,
            CreatedAt = r.CreatedAt,
            ResolvedAt = r.ResolvedAt,
            //InterventionId = r.InterventionId
        };

    
    }
}