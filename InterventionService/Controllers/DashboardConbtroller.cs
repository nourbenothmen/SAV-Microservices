
using InterventionService.Data;
using InterventionService.DTOs;
using InterventionService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;


namespace InterventionService.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize(Roles = "ResponsableSAV")]
    public class DashboardController : ControllerBase
    {
        private readonly InterventionDbContext _context;
        private readonly HttpClient _httpClient;

        public DashboardController(
    InterventionDbContext context,
    IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet("interventions/today")]
        public async Task<IActionResult> GetInterventionsToday()
        {
            var today = DateTime.Today;

            var interventions = await _context.Interventions
                .Where(i => i.DateIntervention.Date == today)
                .OrderBy(i => i.DateIntervention)
                .ToListAsync();

            // 🔐 récupérer le token entrant
            var authHeader = Request.Headers["Authorization"].ToString();

            var result = new List<object>();

            foreach (var i in interventions)
            {
                ClientDTO? client = null;

                try
                {
                    var request = new HttpRequestMessage(
                        HttpMethod.Get,
                        $"https://localhost:7091/apigateway/customers/{i.ClientId}"
                    );

                    // 🔐 propager le token
                    request.Headers.Authorization =
                        AuthenticationHeaderValue.Parse(authHeader);

                    var response = await _httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        client = await response.Content.ReadFromJsonAsync<ClientDTO>();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erreur CustomerService: " + ex.Message);
                }

                result.Add(new
                {
                    id = i.Id,
                    heure = i.DateIntervention.ToString("HH:mm"),
                    technicien = i.TechnicienNom,
                    client = client != null
                        ? client.NomComplet
                        : $"Client #{i.ClientId}",
                    adresse = client?.Address ?? "Non définie",
                    telephone = client?.Telephone,
                    statut = i.Statut
                });
            }

            return Ok(result);
        }



    }
}
