// CustomerService/Controllers/CustomersController.cs
using CustomerService.Models;
using CustomerService.Models.DTOs;
using CustomerService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CustomerService.Controllers
{
    [Route("api/customers")]
    [ApiController]
    [Authorize] // Toutes les routes protégées par JWT
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // Méthode helper pour extraire le userId du token
        private string GetUserIdFromToken()
        {
            // Essayer d'abord "uid" (votre claim custom)
            var userId = User.FindFirst("uid")?.Value;

            // Fallback sur NameIdentifier si "uid" n'existe pas
            if (string.IsNullOrEmpty(userId))
                userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return userId;
        }

        // GET: api/customers
        [HttpGet]
        [Authorize(Roles = "ResponsableSAV")] // Seul le ResponsableSAV peut voir tous les clients
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }

        // GET: api/customers/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomerById(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);

            if (customer == null)
                return NotFound(new { message = "Client non trouvé." });

            // Un client ne peut voir que ses propres données
            var userId = GetUserIdFromToken(); // CORRECTION ICI

            if (User.IsInRole("Client") && customer.UserId.ToString() != userId)
                return Forbid();

            return Ok(customer);
        }

        // GET: api/customers/me
        [HttpGet("me")]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult<CustomerDto>> GetMyProfile()
        {
            var userId = GetUserIdFromToken();

            if (string.IsNullOrEmpty(userId))
            {
                var claims = User.Claims.Select(c => new { c.Type, c.Value });
                return Unauthorized(new
                {
                    message = "UserId non trouvé dans le token",
                    availableClaims = claims
                });
            }

            var customer = await _customerService.GetCustomerByUserIdAsync(userId);

            if (customer == null)
                return NotFound(new
                {
                    message = "Profil client non trouvé.",
                    searchedUserId = userId
                });

            return Ok(customer);
        }

        // POST: api/customers
        [HttpPost("register")]
        [AllowAnonymous] // Pas de token requis
        public async Task<ActionResult<CustomerDto>> RegisterCustomer([FromBody] CreateCustomerRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdCustomer = await _customerService.CreateCustomerAsync(request);
                return CreatedAtAction(nameof(GetCustomerById), new { id = createdCustomer.Id }, createdCustomer);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/customers/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserIdFromToken(); // CORRECTION ICI

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "UserId non trouvé dans le token" });

            var customer = await _customerService.GetCustomerByIdAsync(id);

            if (customer == null)
                return NotFound(new { message = "Client non trouvé." });

            // Un client ne peut modifier que son propre profil
            if (User.IsInRole("Client") && customer.UserId.ToString().ToLower() != userId.ToLower())
            {
                return Forbid(); // 403
            }

            var updated = await _customerService.UpdateCustomerAsync(id, request);
            if (!updated)
                return BadRequest(new { message = "Échec de la mise à jour." });

            return NoContent();
        }

        // DELETE: api/customers/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "ResponsableSAV")] // CORRECTION : Seul ResponsableSAV peut supprimer
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var deleted = await _customerService.DeleteCustomerAsync(id);
            if (!deleted)
                return NotFound(new { message = "Client non trouvé ou déjà supprimé." });

            return NoContent();
        }

        /// <summary>
        /// Récupère l'ID numérique du client à partir du UserId (GUID Identity)
        /// GET: api/customers/client-id?userId=76c3df59-936d-478c-90f4-fc97f76aba93
        /// </summary>
        [HttpGet("client-id")]
        public async Task<ActionResult<int>> GetClientId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { message = "userId est requis." });

            var customer = await _customerService.GetCustomerByUserIdAsync(userId);
            if (customer == null)
                return NotFound(0);

            return Ok(customer.Id);
        }

        /// <summary>
        /// Version "me" pour le client connecté
        /// GET: api/customers/me/client-id
        /// </summary>
        [HttpGet("me/client-id")]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult<int>> GetMyClientId()
        {
            var userId = User.FindFirst("uid")?.Value
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "UserId non trouvé dans le token." });

            var customer = await _customerService.GetCustomerByUserIdAsync(userId);
            if (customer == null)
                return NotFound(0);

            return Ok(customer.Id);
        }
    }
}