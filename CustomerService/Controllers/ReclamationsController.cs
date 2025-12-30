// Controllers/ReclamationsController.cs
using CustomerService.Models;
using CustomerService.Models.DTOs;
using CustomerService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CustomerService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReclamationsController : ControllerBase
    {
        private readonly IReclamationService _service;
        private readonly ILogger<ReclamationsController> _logger;

        public ReclamationsController(IReclamationService service, ILogger<ReclamationsController> logger)
        {
            _service = service;
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