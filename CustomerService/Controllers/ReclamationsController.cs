// Controllers/ReclamationsController.cs
using CustomerService.Models;
using CustomerService.Models.DTOs;
using CustomerService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using CustomerService.Data;

namespace CustomerService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReclamationsController : ControllerBase
    {
        private readonly IReclamationService _service;
        private readonly CustomerDbContext _context;           // ← Ajouté
        private readonly IHttpClientFactory _httpClientFactory; // ← Ajouté
        private readonly HttpClient _httpClient;
        private readonly ILogger<ReclamationsController> _logger;

        public ReclamationsController(
              IReclamationService service,
              CustomerDbContext context,                    // ← Injecté
              IHttpClientFactory httpClientFactory,         // ← Injecté
              ILogger<ReclamationsController> logger)
        {
            _service = service;
            _context = context;
            _httpClientFactory = httpClientFactory;
            _httpClient = httpClientFactory.CreateClient(); // ← Créé ici
            _logger = logger;
        }

        //private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        //private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        private string CurrentUserId => User.FindFirst("uid")!.Value;



        // POST api/reclamations
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReclamationDto>> Create([FromBody] CreateReclamationDto dto)
        {
            try
            {
                var reclamation = await _service.CreateReclamationAsync(CurrentUserId, dto);
                var result = new ReclamationDto
                {
                    Id = reclamation.Id,
                    ArticleId = reclamation.ArticleId,
                    Description = reclamation.Description,
                    Status = reclamation.Status,
                    CreatedAt = reclamation.CreatedAt
                };
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /*
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "ResponsableSAV")]
        public async Task<ActionResult<Reclamation>> UpdateReclamationStatus(int id, [FromBody] UpdateStatutDTO dto)
        {
            try
            {
                var reclamation = await _service.GetReclamationByIdAsync(id);
                if (reclamation == null)
                {
                    return NotFound($"Réclamation avec l'ID {id} non trouvée");
                }

                // Conversion du string en enum ReclamationStatus
                if (!Enum.TryParse<ReclamationStatus>(dto.Statut, true, out var newStatus))
                {
                    return BadRequest("Statut invalide. Valeurs possibles : EnAttente, Planifiée, EnCours, Résolue, etc.");
                }

                reclamation.Status = newStatus; // ← Utilise Status (l'enum existant)
                reclamation.ProcessedAt = DateTime.Now; // ou CreatedAt si tu veux garder une trace

                var updated = await _service.UpdateReclamationAsync(id, reclamation);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du statut de la réclamation {Id}", id);
                return StatusCode(500, "Une erreur est survenue lors de la mise à jour du statut");
            }
        }*/

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "ResponsableSAV")]
        public async Task<ActionResult<Reclamation>> UpdateReclamationStatus(int id, [FromBody] UpdateStatutDTO dto)
        {
            try
            {
                var reclamation = await _service.GetReclamationByIdAsync(id);
                if (reclamation == null)
                {
                    return NotFound($"Réclamation avec l'ID {id} non trouvée");
                }

                // Conversion du string en enum ReclamationStatus
                if (!Enum.TryParse<ReclamationStatus>(dto.Statut, true, out var newStatus))
                {
                    return BadRequest("Statut invalide. Valeurs possibles : EnAttente, Planifiée, EnCours, Terminée");
                }

                reclamation.Status = newStatus;

                // ← NOUVELLE LOGIQUE : Si le nouveau statut est "Terminée", on met ResolvedAt à maintenant
                if (newStatus == ReclamationStatus.Terminée)
                {
                    reclamation.ResolvedAt = DateTime.Now; // ou DateTime.UtcNow si tu préfères UTC
                }
                // Optionnel : si on repasse à un autre statut, on peut effacer ResolvedAt
                else
                {
                    reclamation.ResolvedAt = null;
                }

                reclamation.ProcessedAt = DateTime.Now; // trace de la dernière modification

                var updated = await _service.UpdateReclamationAsync(id, reclamation);

                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du statut de la réclamation {Id}", id);
                return StatusCode(500, "Une erreur est survenue lors de la mise à jour du statut");
            }
        }

        // GET api/reclamations/my
        [HttpGet("my")]
        [Authorize]
        public async Task<ActionResult<List<ReclamationDto>>> GetMyReclamations()
        {
            var reclamations = await _service.GetMyReclamationsAsync(CurrentUserId);
            return Ok(reclamations);
        }

        // GET api/reclamations
        [HttpGet]
        [Authorize(Roles = "ResponsableSAV")]
        public async Task<ActionResult<List<ReclamationDto>>> GetAll()
        {
            var reclamations = await _service.GetAllReclamationsAsync();
            return Ok(reclamations);
        }
        [HttpGet("{id}/details")]
        [Authorize] // On garde l'authentification
        public async Task<ActionResult<ReclamationDetailsDto>> GetReclamationDetails(int id)
        {
            var userIdClaim = User.FindFirst("uid");
            if (userIdClaim == null) return Unauthorized();

            var userId = userIdClaim.Value;
            var isResponsableSAV = User.IsInRole("ResponsableSAV");

            int? customerId = null;

            // Si ce n'est PAS un ResponsableSAV → on vérifie que c'est bien son client
            if (!isResponsableSAV)
            {
                customerId = await _context.Customers
                    .Where(c => c.UserId == userId)
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync();

                if (customerId == 0) return NotFound("Client non trouvé");
            }

            // Recherche de la réclamation
            var query = _context.Reclamations.AsQueryable();

            if (!isResponsableSAV && customerId.HasValue)
            {
                query = query.Where(r => r.Id == id && r.CustomerId == customerId.Value);
            }
            else
            {
                query = query.Where(r => r.Id == id); // ResponsableSAV voit tout
            }

            var reclamation = await query.FirstOrDefaultAsync();

            if (reclamation == null)
                return NotFound("Réclamation non trouvée ou non autorisée");

            // Le reste du code (nom article + intervention) reste IDENTIQUE
            var dto = new ReclamationDetailsDto
            {
                Id = reclamation.Id,
                Description = reclamation.Description,
                ProblemType = reclamation.ProblemType ?? "Non spécifié",
                Status = reclamation.Status.ToString(),
                CreatedAt = reclamation.CreatedAt,
                DesiredInterventionDate = reclamation.DesiredInterventionDate
            };

            // Autorisation pour les appels API
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
                    HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", ""));

            // Nom de l'article
            try
            {
                var articleResponse = await _httpClient.GetAsync($"https://localhost:7091/apigateway/articles/{reclamation.ArticleId}");
                if (articleResponse.IsSuccessStatusCode)
                {
                    var article = await articleResponse.Content.ReadFromJsonAsync<JsonElement>();
                    var marque = article.TryGetProperty("marque", out var m) ? m.GetString() : "";
                    var nom = article.TryGetProperty("nom", out var n) ? n.GetString() : "";
                    var modele = article.TryGetProperty("modele", out var mo) ? mo.GetString() : "";

                    dto.ArticleNom = string.Join(" - ", new[] { marque, nom, modele != null ? $"Modèle {modele}" : null }
                        .Where(s => !string.IsNullOrWhiteSpace(s))) ?? "Article inconnu";
                }
            }
            catch { dto.ArticleNom = "Article inconnu"; }

            // Intervention
            try
            {
                var interventionResponse = await _httpClient.GetAsync(
                    $"https://localhost:7091/apigateway/interventions/by-reclamation/{id}");

                if (interventionResponse.IsSuccessStatusCode)
                {
                    var interventions = await interventionResponse.Content.ReadFromJsonAsync<List<JsonElement>>();
                    var intervention = interventions?.FirstOrDefault();

                    if (intervention.HasValue)
                    {
                        dto.TechnicienNom = intervention.Value.TryGetProperty("TechnicienNom", out var tn)
                            ? tn.GetString() ?? "Non assigné"
                            : "Non assigné";

                        if (intervention.Value.TryGetProperty("DateIntervention", out var di))
                            dto.DateIntervention = di.GetDateTime();

                        if (intervention.Value.TryGetProperty("EstSousGarantie", out var eg))
                            dto.EstSousGarantie = eg.GetBoolean();

                        if (intervention.Value.TryGetProperty("MontantTotal", out var mt) &&
                            reclamation.Status == ReclamationStatus.Terminée && !dto.EstSousGarantie)
                        {
                            dto.MontantTotal = mt.GetDecimal();
                        }
                    }
                }
            }
            catch { }

            return Ok(dto);
        }


        // GET api/reclamations/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ReclamationDto>> GetById(int id)
        {
            var reclamation = await _service.GetReclamationByIdAsync(id);
            if (reclamation == null) return NotFound();

            // Vérifier que c'est bien son reclamation ou qu'il est ResponsableSAV
            var isOwner = reclamation.Customer.UserId == CurrentUserId;
            var isAdmin = User.IsInRole("ResponsableSAV");
            if (!isOwner && !isAdmin) return Forbid();

            return Ok(new ReclamationDto
            {
                Id = reclamation.Id,
                ArticleId = reclamation.ArticleId,
                Description = reclamation.Description,
                Status = reclamation.Status,
                CreatedAt = reclamation.CreatedAt,
                ResolvedAt = reclamation.ResolvedAt,
                //InterventionId = reclamation.InterventionId
            });
        }
    }
}