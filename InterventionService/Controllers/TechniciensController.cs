using InterventionService.Models;
using InterventionService.Models.InterventionService.Models;
using InterventionService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterventionService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TechniciensController : ControllerBase
    {
        private readonly TechnicienService _technicienService;

        public TechniciensController(TechnicienService technicienService)
        {
            _technicienService = technicienService;
        }

        [HttpGet]
        [Authorize(Roles = "ResponsableSAV")]
        public async Task<IActionResult> GetAll()
        {
            var techniciens = await _technicienService.GetAllTechniciensAsync();
            return Ok(techniciens);
        }

        [HttpGet("{id}")]
       
        public async Task<IActionResult> Get(int id)
        {
            var technicien = await _technicienService.GetTechnicienByIdAsync(id);
            if (technicien == null) return NotFound();
            return Ok(technicien);
        }

        [HttpPost]
        [Authorize(Roles = "ResponsableSAV")]
        public async Task<IActionResult> Create([FromBody] Technicien technicien)
        {
            var created = await _technicienService.CreateTechnicienAsync(technicien);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ResponsableSAV")]
        public async Task<IActionResult> Update(int id, [FromBody] Technicien technicien)
        {
            if (id != technicien.Id) return BadRequest();
            var updated = await _technicienService.UpdateTechnicienAsync(technicien);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ResponsableSAV")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _technicienService.DeleteTechnicienAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
