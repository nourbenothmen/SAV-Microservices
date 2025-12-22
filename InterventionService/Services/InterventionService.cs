using Microsoft.EntityFrameworkCore;
using InterventionService.Data;
using InterventionService.Models;

namespace InterventionService.Services
{
    public class InterventionServiceImpl : IInterventionService
    {
        private readonly InterventionDbContext _context;
        private readonly ILogger<InterventionServiceImpl> _logger;

        public InterventionServiceImpl(InterventionDbContext context, ILogger<InterventionServiceImpl> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Intervention>> GetAllInterventionsAsync()
        {
            try
            {
                return await _context.Interventions
                    .Include(i => i.Pieces)
                    .OrderByDescending(i => i.DateCreation)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des interventions");
                throw;
            }
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
    }
}