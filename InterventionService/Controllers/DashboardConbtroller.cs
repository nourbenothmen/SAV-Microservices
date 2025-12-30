
using InterventionService.Data;
using InterventionService.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

namespace InterventionService.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize(Roles = "ResponsableSAV")]
    public class DashboardController : ControllerBase
    {
        private readonly InterventionDbContext _context;

        public DashboardController(InterventionDbContext context)
        {
            _context = context;
        }

        [HttpGet("interventions/today")]
        public async Task<IActionResult> GetInterventionsToday()
        {
            var today = DateTime.Today;

            var interventions = await _context.Interventions
                .Where(i => i.DateIntervention.Date == today)
                .OrderBy(i => i.DateIntervention)
                .Select(i => new
                {
                    heure = i.DateIntervention.ToString("HH:mm"),
                    technicien = i.TechnicienNom,
                    client = "Client #" + i.ClientId, // temporaire
                    adresse = "Non définie",
                    statut = i.Statut,
                    id = i.Id
                })
                .ToListAsync();

            return Ok(interventions);
        }
    }
}
