using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ArticleService.Models;
using ArticleService.Services;

namespace ArticleService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Ajout de l'autorisation globale
    public class CustomerArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;
        private readonly ILogger<CustomerArticlesController> _logger;

        public CustomerArticlesController(IArticleService articleService, ILogger<CustomerArticlesController> logger)
        {
            _articleService = articleService;
            _logger = logger;
        }

        /// <summary>
        /// Récupère tous les articles clients
        /// GET: api/CustomerArticles
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")] // Seulement les admins et managers
        [ProducesResponseType(typeof(IEnumerable<CustomerArticle>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CustomerArticle>>> GetAllCustomerArticles()
        {
            try
            {
                var customerArticles = await _articleService.GetAllCustomerArticlesAsync();
                return Ok(customerArticles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des articles clients");
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération des articles clients", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère un article client par son ID
        /// GET: api/CustomerArticles/5
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CustomerArticle), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CustomerArticle>> GetCustomerArticleById(int id)
        {
            try
            {
                var customerArticle = await _articleService.GetCustomerArticleByIdAsync(id);
                if (customerArticle == null)
                {
                    return NotFound(new { message = $"Article client avec l'ID {id} non trouvé" });
                }
                return Ok(customerArticle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'article client {Id}", id);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération de l'article client", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère tous les articles d'un client spécifique
        /// GET: api/CustomerArticles/client/5
        /// </summary>
        [HttpGet("client/{clientId}")]
        [Authorize(Roles = "Admin,Manager,Employee")] // Admins, managers et employés
        [ProducesResponseType(typeof(IEnumerable<CustomerArticle>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CustomerArticle>>> GetCustomerArticlesByClientId(int clientId)
        {
            try
            {
                var customerArticles = await _articleService.GetCustomerArticlesByClientIdAsync(clientId);
                return Ok(customerArticles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des articles du client {ClientId}", clientId);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération des articles du client", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère tous les clients ayant acheté un article spécifique
        /// GET: api/CustomerArticles/article/5
        /// </summary>
        [HttpGet("article/{articleId}")]
        [Authorize(Roles = "Admin,Manager")] // Seulement les admins et managers
        [ProducesResponseType(typeof(IEnumerable<CustomerArticle>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CustomerArticle>>> GetCustomerArticlesByArticleId(int articleId)
        {
            try
            {
                var customerArticles = await _articleService.GetCustomerArticlesByArticleIdAsync(articleId);
                return Ok(customerArticles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des clients ayant l'article {ArticleId}", articleId);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération des données", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère un article client par son numéro de série
        /// GET: api/CustomerArticles/serial/SN123456789
        /// </summary>
        [HttpGet("serial/{numeroSerie}")]
        [AllowAnonymous] // Accessible sans authentification pour les clients
        [ProducesResponseType(typeof(CustomerArticle), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CustomerArticle>> GetCustomerArticleBySerialNumber(string numeroSerie)
        {
            try
            {
                var customerArticle = await _articleService.GetCustomerArticleBySerialNumberAsync(numeroSerie);
                if (customerArticle == null)
                {
                    return NotFound(new { message = $"Article client avec le numéro de série {numeroSerie} non trouvé" });
                }
                return Ok(customerArticle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'article client par numéro de série {NumeroSerie}", numeroSerie);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération de l'article client", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère tous les articles sous garantie d'un client
        /// GET: api/CustomerArticles/client/5/sous-garantie
        /// </summary>
        [HttpGet("client/{clientId}/sous-garantie")]
        [Authorize(Roles = "ResponsableSAV,Client")] // Multiple rôles autorisés
        [ProducesResponseType(typeof(IEnumerable<CustomerArticle>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CustomerArticle>>> GetCustomerArticlesSousGarantie(int clientId)
        {
            try
            {
                // Vérification pour les clients : ils ne peuvent voir que leurs propres articles
                var userRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

                if (userRole == "Client" && userIdClaim != null)
                {
                    if (int.Parse(userIdClaim) != clientId)
                    {
                        return Forbid(); // Le client essaie d'accéder aux données d'un autre client
                    }
                }

                var customerArticles = await _articleService.GetCustomerArticlesSousGarantieAsync(clientId);
                return Ok(customerArticles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des articles sous garantie du client {ClientId}", clientId);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération des articles sous garantie", error = ex.Message });
            }
        }

        /// <summary>
        /// Crée un nouvel article client (enregistre l'achat d'un client)
        /// POST: api/CustomerArticles
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Employee")] // Admins, managers et employés
        [ProducesResponseType(typeof(CustomerArticle), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CustomerArticle>> CreateCustomerArticle([FromBody] CustomerArticle customerArticle)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdCustomerArticle = await _articleService.CreateCustomerArticleAsync(customerArticle);
                return CreatedAtAction(nameof(GetCustomerArticleById), new { id = createdCustomerArticle.Id }, createdCustomerArticle);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'article client");
                return StatusCode(500, new { message = "Une erreur est survenue lors de la création de l'article client", error = ex.Message });
            }
        }

        /// <summary>
        /// Met à jour un article client existant
        /// PUT: api/CustomerArticles/5
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")] // Seulement les admins et managers
        [ProducesResponseType(typeof(CustomerArticle), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CustomerArticle>> UpdateCustomerArticle(int id, [FromBody] CustomerArticle customerArticle)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedCustomerArticle = await _articleService.UpdateCustomerArticleAsync(id, customerArticle);
                if (updatedCustomerArticle == null)
                {
                    return NotFound(new { message = $"Article client avec l'ID {id} non trouvé" });
                }

                return Ok(updatedCustomerArticle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de l'article client {Id}", id);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la mise à jour de l'article client", error = ex.Message });
            }
        }

        /// <summary>
        /// Supprime un article client
        /// DELETE: api/CustomerArticles/5
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Seulement les admins
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteCustomerArticle(int id)
        {
            try
            {
                var result = await _articleService.DeleteCustomerArticleAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Article client avec l'ID {id} non trouvé" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'article client {Id}", id);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la suppression de l'article client", error = ex.Message });
            }
        }

        /// <summary>
        /// Vérifie si un article client est sous garantie
        /// GET: api/CustomerArticles/5/verifier-garantie
        /// </summary>
        [HttpGet("{id}/verifier-garantie")]
        [AllowAnonymous] // Accessible sans authentification pour les clients
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> VerifierGarantie(int id)
        {
            try
            {
                var estSousGarantie = await _articleService.VerifierGarantieAsync(id);
                return Ok(new { customerArticleId = id, estSousGarantie });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de la garantie {Id}", id);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la vérification de la garantie", error = ex.Message });
            }
        }
    }
}