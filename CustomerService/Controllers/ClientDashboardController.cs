using CustomerService.Data;
using CustomerService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Http.Json;


namespace CustomerService.Controllers
{
    [ApiController]
    [Route("api/dashboard/client")]
    public class ClientDashboardController : ControllerBase
    {
        private readonly CustomerDbContext _context;
        private readonly HttpClient _httpClient;
        public ClientDashboardController(CustomerDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }
[HttpGet("stats")]
    [Authorize]
    public async Task<IActionResult> GetClientStats()
    {
        var userIdClaim = User.FindFirst("uid");
        if (userIdClaim == null)
            return Unauthorized();

        var userId = userIdClaim.Value;

        var customerId = await _context.Customers
            .Where(c => c.UserId == userId)
            .Select(c => c.Id)
            .FirstOrDefaultAsync();

        if (customerId == 0)
            return NotFound("Client introuvable");

        // Réclamations
        var totalReclamations = await _context.Reclamations
            .CountAsync(r => r.CustomerId == customerId);

        var enCours = await _context.Reclamations
            .CountAsync(r => r.CustomerId == customerId &&
                (r.Status == ReclamationStatus.EnCours ||
                 r.Status == ReclamationStatus.Planifiée));

        var terminees = await _context.Reclamations
            .CountAsync(r => r.CustomerId == customerId &&
                 r.Status == ReclamationStatus.Terminée);

        // Articles via API
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", ""));

        var articlesResponse = await _httpClient.GetAsync("https://localhost:7091/apigateway/articles/my");
        int articlesCount = 0;

        if (articlesResponse.IsSuccessStatusCode)
        {
            // Lire le JSON en utilisant ReadFromJsonAsync (natif .NET 8)
            var articles = await articlesResponse.Content.ReadFromJsonAsync<List<object>>();
            articlesCount = articles?.Count ?? 0;
        }

        return Ok(new
        {
            totalReclamations,
            enCours,
            terminees,
            articles = articlesCount
        });
    }


}

}