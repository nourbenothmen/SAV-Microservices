using CustomerService.Data;
using CustomerService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize(Roles = "ResponsableSAV")]
    public class DashboardController : ControllerBase
    {
        private readonly CustomerDbContext _context;

        public DashboardController(CustomerDbContext context)
        {
            _context = context;
        }

        [HttpGet("reclamations/stats")]
        public async Task<IActionResult> GetReclamationsStats()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var actives = await _context.Reclamations
                .CountAsync(r =>
                    r.Status == ReclamationStatus.Planifiée ||
                    r.Status == ReclamationStatus.EnCours
                );

            var enAttente = await _context.Reclamations
                .CountAsync(r => r.Status == ReclamationStatus.EnAttente);

            var termineesCeMois = await _context.Reclamations
                .CountAsync(r =>
                    r.Status == ReclamationStatus.Terminée &&
                    r.ResolvedAt.HasValue &&
                    r.ResolvedAt.Value.Month == currentMonth &&
                    r.ResolvedAt.Value.Year == currentYear
                );

            return Ok(new
            {
                actives,
                enAttente,
                termineesCeMois
            });
        }
    }
}
