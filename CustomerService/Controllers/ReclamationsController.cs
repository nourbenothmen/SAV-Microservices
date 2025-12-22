// Controllers/ReclamationsController.cs
using CustomerService.Models;
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