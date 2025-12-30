using InterventionService.Data;
using InterventionService.DTOs;
using InterventionService.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace InterventionService.Services
{
    public class InterventionServiceImpl : IInterventionService
    {
        private readonly InterventionDbContext _context;
        private readonly ILogger<InterventionServiceImpl> _logger;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InterventionServiceImpl(
         InterventionDbContext context,
         ILogger<InterventionServiceImpl> logger,
         HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<InterventionListDTO>> GetAllInterventionsAsync()
        {
            var interventions = await _context.Interventions
                .OrderByDescending(i => i.DateCreation)
                .ToListAsync();

            var result = new List<InterventionListDTO>();

            foreach (var i in interventions)
            {
                // 🔗 Appels aux autres microservices
                var clientNom = await GetClientName(i.ClientId);
                var articleNom = await GetArticleName(i.ArticleId);

                result.Add(new InterventionListDTO
                {
                    Id = i.Id,
                    ReclamationId = i.ReclamationId,          // ✅ Ajouter ReclamationId
                    ClientId = i.ClientId,
                    ClientNom = clientNom,
                    ArticleId = i.ArticleId,
                    ArticleNom = articleNom,
                    EstSousGarantie = i.EstSousGarantie,      // ✅ Ajouter EstSousGarantie
                    TechnicienNom = i.TechnicienNom,
                    Statut = i.Statut,
                    DateCreation = i.DateCreation
                });

            }

            return result;
        }


        public async Task<InterventionDTO?> GetInterventionDetailsAsync(int id)
        {
            var intervention = await _context.Interventions
                .Include(i => i.Pieces)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (intervention == null) return null;

            var clientNom = await GetClientName(intervention.ClientId);
            var articleNom = await GetArticleName(intervention.ArticleId);

            // Optionnel : détails client complet
            var clientDetails = await GetClientDetails(intervention.ClientId);

            return new InterventionDTO
            {
                Id = intervention.Id,
                ReclamationId = intervention.ReclamationId,
                ClientId = intervention.ClientId,
                ClientNom = clientNom,
                ClientAddress = clientDetails?.Address ?? "-",
                ClientTelephone = clientDetails?.Telephone ?? "-",
                ArticleId = intervention.ArticleId,
                ArticleNom = articleNom,
                EstSousGarantie = intervention.EstSousGarantie,
                TechnicienNom = intervention.TechnicienNom,
                Statut = intervention.Statut,
                DateIntervention = intervention.DateIntervention,
                DateCreation = intervention.DateCreation,
                Description = intervention.Description,

                DureeIntervention = intervention.DureeIntervention,
                TarifHoraire = intervention.TarifHoraire,
                MontantDeplacement = intervention.MontantDeplacement,
                TauxTVA = intervention.TauxTVA,
                ModePaiement = intervention.ModePaiement,
                StatutPaiement = intervention.StatutPaiement,
                MontantMainOeuvre = intervention.MontantMainOeuvre,
                MontantTotal = intervention.MontantTotal,

                Pieces = intervention.Pieces.Select(p => new InterventionPartDTO
                {
                    Id = p.Id,
                    InterventionId = p.InterventionId,
                    NomPiece = p.NomPiece,
                    Reference = p.Reference,
                    Quantite = p.Quantite,
                    PrixUnitaire = p.PrixUnitaire,
                    PrixTotal = p.PrixTotal,
                    Description = p.Description
                }).ToList()
            };
        }
        public async Task<Intervention?> GetInterventionByIdAsync(int id)
        {
            try
            {
                return await _context.Interventions
                    .Include(i => i.Pieces)
                    .FirstOrDefaultAsync(i => i.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'intervention {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Intervention>> GetInterventionsByClientIdAsync(int clientId)
        {
            try
            {
                return await _context.Interventions
                    .Include(i => i.Pieces)
                    .Where(i => i.ClientId == clientId)
                    .OrderByDescending(i => i.DateCreation)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des interventions du client {ClientId}", clientId);
                throw;
            }
        }

        public async Task<IEnumerable<Intervention>> GetInterventionsByReclamationIdAsync(int reclamationId)
        {
            try
            {
                return await _context.Interventions
                    .Include(i => i.Pieces)
                    .Where(i => i.ReclamationId == reclamationId)
                    .OrderByDescending(i => i.DateCreation)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des interventions de la réclamation {ReclamationId}", reclamationId);
                throw;
            }
        }

        public async Task<Intervention> CreateInterventionAsync(Intervention intervention)
        {
            try
            {
                intervention.DateCreation = DateTime.Now;
                intervention.MontantTotal = await CalculateInterventionCostAsync(intervention);

                _context.Interventions.Add(intervention);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Intervention créée avec succès: {Id}", intervention.Id);
                return intervention;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'intervention");
                throw;
            }
        }

        public async Task<Intervention?> UpdateInterventionAsync(int id, Intervention intervention)
        {
            try
            {
                var existingIntervention = await _context.Interventions
                    .Include(i => i.Pieces)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (existingIntervention == null)
                {
                    return null;
                }

                existingIntervention.Description = intervention.Description;
                existingIntervention.DateIntervention = intervention.DateIntervention;
                existingIntervention.Statut = intervention.Statut;
                existingIntervention.EstSousGarantie = intervention.EstSousGarantie;
                existingIntervention.MontantMainOeuvre = intervention.MontantMainOeuvre;
                existingIntervention.Commentaire = intervention.Commentaire;
                existingIntervention.TechnicienNom = intervention.TechnicienNom;
                existingIntervention.DateMiseAJour = DateTime.Now;

                existingIntervention.MontantTotal = await CalculateInterventionCostAsync(existingIntervention);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Intervention mise à jour avec succès: {Id}", id);
                return existingIntervention;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de l'intervention {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteInterventionAsync(int id)
        {
            try
            {
                var intervention = await _context.Interventions.FindAsync(id);
                if (intervention == null)
                {
                    return false;
                }

                _context.Interventions.Remove(intervention);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Intervention supprimée avec succès: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'intervention {Id}", id);
                throw;
            }
        }

        public async Task<InterventionPart> AddPartToInterventionAsync(int interventionId, InterventionPart part)
        {
            try
            {
                var intervention = await _context.Interventions
                    .Include(i => i.Pieces)
                    .FirstOrDefaultAsync(i => i.Id == interventionId);

                if (intervention == null)
                {
                    throw new ArgumentException("Intervention non trouvée", nameof(interventionId));
                }

                part.InterventionId = interventionId;
                part.PrixTotal = part.Quantite * part.PrixUnitaire;

                _context.InterventionParts.Add(part);

                // Recalculer le montant total de l'intervention
                intervention.MontantTotal = await CalculateInterventionCostAsync(intervention);
                intervention.DateMiseAJour = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Pièce ajoutée à l'intervention {InterventionId}", interventionId);
                return part;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'ajout d'une pièce à l'intervention {InterventionId}", interventionId);
                throw;
            }
        }

        public async Task<bool> RemovePartFromInterventionAsync(int interventionId, int partId)
        {
            try
            {
                var part = await _context.InterventionParts
                    .FirstOrDefaultAsync(p => p.Id == partId && p.InterventionId == interventionId);

                if (part == null)
                {
                    return false;
                }

                _context.InterventionParts.Remove(part);

                // Recalculer le montant total de l'intervention
                var intervention = await _context.Interventions
                    .Include(i => i.Pieces)
                    .FirstOrDefaultAsync(i => i.Id == interventionId);

                if (intervention != null)
                {
                    intervention.MontantTotal = await CalculateInterventionCostAsync(intervention);
                    intervention.DateMiseAJour = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Pièce supprimée de l'intervention {InterventionId}", interventionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression d'une pièce de l'intervention {InterventionId}", interventionId);
                throw;
            }
        }

        public async Task<decimal> CalculateTotalCostAsync(int interventionId)
        {
            try
            {
                var intervention = await _context.Interventions
                    .Include(i => i.Pieces)
                    .FirstOrDefaultAsync(i => i.Id == interventionId);

                if (intervention == null)
                {
                    throw new ArgumentException("Intervention non trouvée", nameof(interventionId));
                }

                return await CalculateInterventionCostAsync(intervention);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du calcul du coût total de l'intervention {InterventionId}", interventionId);
                throw;
            }
        }

        private async Task<decimal> CalculateInterventionCostAsync(Intervention intervention)
        {
            // Si sous garantie, l'intervention est gratuite
            if (intervention.EstSousGarantie)
            {
                return 0;
            }

            // Calculer le coût total des pièces
            var totalPieces = intervention.Pieces?.Sum(p => p.PrixTotal) ?? 0;

            // Ajouter le coût de la main d'œuvre
            return totalPieces + intervention.MontantMainOeuvre;
        }

        private string? GetBearerToken()
        {
            return _httpContextAccessor.HttpContext?
                .Request.Headers["Authorization"]
                .FirstOrDefault();
        }

        private async Task<string> GetClientName(int clientId)
        {
            if (clientId <= 0)
                return "-";

            var token = _httpContextAccessor.HttpContext?
                .Request.Headers["Authorization"]
                .ToString()
                .Replace("Bearer ", "");

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://localhost:7091/apigateway/customers/{clientId}"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            var body = await response.Content.ReadAsStringAsync();

            _logger.LogInformation(
                "CLIENT API STATUS: {Status} | BODY: {Body}",
                response.StatusCode,
                body
            );

            if (!response.IsSuccessStatusCode)
                return "-";

            var client = JsonSerializer.Deserialize<ClientDTO>(
                body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return client?.NomComplet ?? "-";
        }

        private async Task<string> GetArticleName(int articleId)
        {
            var token = GetBearerToken();
            if (string.IsNullOrEmpty(token))
                return "-";

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://localhost:7091/apigateway/articles/{articleId}"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return "-";

            var article = await response.Content.ReadFromJsonAsync<ArticleDTO>();

            return article?.DisplayName ?? "-";
        }

        public async Task<InterventionPart> AddOrUpdatePartAsync(InterventionPart part)
        {
            if (part.Id == 0)
            {
                part.PrixTotal = part.Quantite * part.PrixUnitaire;
                _context.InterventionParts.Add(part);
            }
            else
            {
                var existing = await _context.InterventionParts.FindAsync(part.Id);
                if (existing == null) return null;

                existing.NomPiece = part.NomPiece;
                existing.Reference = part.Reference;
                existing.Quantite = part.Quantite;
                existing.PrixUnitaire = part.PrixUnitaire;
                existing.PrixTotal = part.Quantite * part.PrixUnitaire;
                existing.Description = part.Description;

                _context.InterventionParts.Update(existing);
            }

            await _context.SaveChangesAsync();
            return part;
        }

        public async Task<bool> DeletePartAsync(int partId)
        {
            var part = await _context.InterventionParts.FindAsync(partId);
            if (part == null) return false;

            _context.InterventionParts.Remove(part);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Intervention> CloseInterventionAsync(int interventionId, CloseInterventionDTO dto)
        {
            var intervention = await _context.Interventions
                .Include(i => i.Pieces)
                .FirstOrDefaultAsync(i => i.Id == interventionId);

            if (intervention == null) return null;

            // Mise à jour des champs
            intervention.DureeIntervention = dto.DureeIntervention;
            intervention.TarifHoraire = dto.TarifHoraire;
            intervention.MontantDeplacement = dto.MontantDeplacement;
            intervention.TauxTVA = dto.TauxTVA;
            intervention.ModePaiement = dto.ModePaiement;
            intervention.StatutPaiement = dto.StatutPaiement;
            intervention.Statut = "Terminée";
            intervention.DateMiseAJour = DateTime.Now;

            // Calculs automatiques
            intervention.MontantMainOeuvre = intervention.DureeIntervention * intervention.TarifHoraire;
            decimal totalPieces = intervention.Pieces.Sum(p => p.PrixTotal);
            decimal totalHT = totalPieces + intervention.MontantMainOeuvre + intervention.MontantDeplacement;
            decimal montantTVA = totalHT * intervention.TauxTVA;
            intervention.MontantTotal = totalHT + montantTVA;

            await _context.SaveChangesAsync();
            return intervention;
        }

        private async Task<ClientDTO?> GetClientDetails(int clientId)
        {
            if (clientId <= 0) return null;

            var token = GetBearerToken()?.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token)) return null;

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:7091/apigateway/customers/{clientId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<ClientDTO>();
        }

    }
}