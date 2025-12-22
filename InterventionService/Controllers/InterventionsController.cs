using Microsoft.AspNetCore.Mvc;
using InterventionService.Models;
using InterventionService.Services;
using Microsoft.AspNetCore.Authorization;

namespace InterventionService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // Protéger toutes les routes
    public class InterventionsController : ControllerBase
    {
        private readonly IInterventionService _interventionService;
        private readonly ILogger<InterventionsController> _logger;

        public InterventionsController(IInterventionService interventionService, ILogger<InterventionsController> logger)
        {
            _interventionService = interventionService;
            _logger = logger;
        }

        /// <summary>
        /// Récupère toutes les interventions (Admin et Responsable SAV seulement)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "ResponsableSAV")]
        public async Task<ActionResult<IEnumerable<Intervention>>> GetAllInterventions()
        {
            try
            {
                var interventions = await _interventionService.GetAllInterventionsAsync();
                return Ok(interventions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des interventions");
                return StatusCode(500, "Une erreur est survenue lors de la récupération des interventions");
            }
        }

        /// <summary>
        /// Récupère une intervention par son ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Intervention>> GetInterventionById(int id)
        {
            try
            {
                // Vérifier si l'utilisateur a le droit de voir cette intervention
                var userIdClaim = User.FindFirst("UserId")?.Value;
                var userRole = User.FindFirst("Role")?.Value;

                var intervention = await _interventionService.GetInterventionByIdAsync(id);
                if (intervention == null)
                {
                    return NotFound($"Intervention avec l'ID {id} non trouvée");
                }

                // Un client ne peut voir que ses propres interventions
                if (userRole == "Client" && userIdClaim != null)
                {
                    if (intervention.ClientId != int.Parse(userIdClaim))
                    {
                        return Forbid();
                    }
                }

                return Ok(intervention);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'intervention {Id}", id);
                return StatusCode(500, "Une erreur est survenue lors de la récupération de l'intervention");
            }
        }

        /// <summary>
        /// Récupère les interventions d'un client
        /// </summary>
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<IEnumerable<Intervention>>> GetInterventionsByClientId(int clientId)
        {
            try
            {
                // Vérifier si l'utilisateur a le droit de voir ces interventions
                var userIdClaim = User.FindFirst("UserId")?.Value;
                var userRole = User.FindFirst("Role")?.Value;

                // Un client ne peut voir que ses propres interventions
                if (userRole == "Client" && userIdClaim != null)
                {
                    if (clientId != int.Parse(userIdClaim))
                    {
                        return Forbid();
                    }
                }

                var interventions = await _interventionService.GetInterventionsByClientIdAsync(clientId);
                return Ok(interventions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des interventions du client {ClientId}", clientId);
                return StatusCode(500, "Une erreur est survenue lors de la récupération des interventions");
            }
        }

        /// <summary>
        /// Crée une nouvelle intervention (Techniciens et Responsable SAV seulement)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "ResponsableSAV")]
        public async Task<ActionResult<Intervention>> CreateIntervention([FromBody] Intervention intervention)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Récupérer le nom du technicien depuis le token si disponible
                var technicienNom = User.FindFirst("FullName")?.Value;
                if (!string.IsNullOrEmpty(technicienNom) && intervention.TechnicienNom == string.Empty)
                {
                    intervention.TechnicienNom = technicienNom;
                }

                var createdIntervention = await _interventionService.CreateInterventionAsync(intervention);
                return CreatedAtAction(nameof(GetInterventionById), new { id = createdIntervention.Id }, createdIntervention);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'intervention");
                return StatusCode(500, "Une erreur est survenue lors de la création de l'intervention");
            }
        }

        /// <summary>
        /// Met à jour une intervention existante (Techniciens et Responsable SAV seulement)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "ResponsableSAV")]
        public async Task<ActionResult<Intervention>> UpdateIntervention(int id, [FromBody] Intervention intervention)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedIntervention = await _interventionService.UpdateInterventionAsync(id, intervention);
                if (updatedIntervention == null)
                {
                    return NotFound($"Intervention avec l'ID {id} non trouvée");
                }

                return Ok(updatedIntervention);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de l'intervention {Id}", id);
                return StatusCode(500, "Une erreur est survenue lors de la mise à jour de l'intervention");
            }
        }

        /// <summary>
        /// Supprime une intervention (Admin seulement)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ResponsableSAV")]
        public async Task<ActionResult> DeleteIntervention(int id)
        {
            try
            {
                var result = await _interventionService.DeleteInterventionAsync(id);
                if (!result)
                {
                    return NotFound($"Intervention avec l'ID {id} non trouvée");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'intervention {Id}", id);
                return StatusCode(500, "Une erreur est survenue lors de la suppression de l'intervention");
            }
        }

        /// <summary>
        /// Ajoute une pièce à une intervention (Techniciens seulement)
        /// </summary>
        [HttpPost("{interventionId}/parts")]
        [Authorize(Roles = "ResponsableSAV")]
        public async Task<ActionResult<InterventionPart>> AddPartToIntervention(int interventionId, [FromBody] InterventionPart part)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var addedPart = await _interventionService.AddPartToInterventionAsync(interventionId, part);
                return CreatedAtAction(nameof(GetInterventionById), new { id = interventionId }, addedPart);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'ajout d'une pièce à l'intervention {InterventionId}", interventionId);
                return StatusCode(500, "Une erreur est survenue lors de l'ajout de la pièce");
            }
        }

        /// <summary>
        /// Calcule le coût total d'une intervention
        /// </summary>
        [HttpGet("{interventionId}/total-cost")]
        public async Task<ActionResult<decimal>> CalculateTotalCost(int interventionId)
        {
            try
            {
                // Vérifier les permissions
                var userIdClaim = User.FindFirst("UserId")?.Value;
                var userRole = User.FindFirst("Role")?.Value;

                var intervention = await _interventionService.GetInterventionByIdAsync(interventionId);
                if (intervention == null)
                {
                    return NotFound($"Intervention avec l'ID {interventionId} non trouvée");
                }

                // Un client ne peut voir que ses propres coûts
                if (userRole == "Client" && userIdClaim != null)
                {
                    if (intervention.ClientId != int.Parse(userIdClaim))
                    {
                        return Forbid();
                    }
                }

                var totalCost = await _interventionService.CalculateTotalCostAsync(interventionId);
                return Ok(new { interventionId, totalCost });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du calcul du coût total de l'intervention {InterventionId}", interventionId);
                return StatusCode(500, "Une erreur est survenue lors du calcul du coût");
            }
        }
    }
}