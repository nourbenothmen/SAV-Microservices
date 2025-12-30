using ArticleService.Models;
using ArticleService.Models.DTO;
using ArticleService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        [Authorize(Roles = "ResponsableSAV")] // Seulement les admins et managers
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
        [Authorize(Roles = "ResponsableSAV")]
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
        [Authorize(Roles = "ResponsableSAV")]
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
        [Authorize(Roles = "ResponsableSAV")]
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
        [Authorize(Roles = "ResponsableSAV")] // Seulement les admins et managers
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
        [Authorize(Roles = "ResponsableSAV")]
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
        /// <summary>
        /// Récupère les articles appartenant au client connecté (utilisé pour le formulaire de réclamation)
        /// GET: api/CustomerArticles/my
        /// </summary>
        /// <summary>
        /// Récupère les articles appartenant au client connecté (utilisé pour le formulaire de réclamation)
        /// GET: api/CustomerArticles/my
        /// </summary>
        [HttpGet("my")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(typeof(IEnumerable<MyArticleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<MyArticleDto>>> GetMyArticles()
        {
            try
            {
                int clientId = GetClientIdFromClaims();

                _logger.LogInformation("Récupération des articles pour le client ID : {ClientId}", clientId);

                var articles = await _articleService.GetMyArticlesAsync(clientId);

                return Ok(articles);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Accès non autorisé à /my - Claim userId manquant ou invalide");
                return Unauthorized(new { message = "Token invalide ou utilisateur non identifié." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur serveur lors de la récupération des articles du client connecté");
                return StatusCode(500, new { message = "Une erreur est survenue lors du chargement de vos articles." });
            }
        }

        /// <summary>
        /// Extrait l'ID du client (ClientId numérique de la table Customers) depuis les claims du JWT
        /// </summary>
        /// <returns>L'ID du client sous forme d'entier</returns>
        /// <exception cref="UnauthorizedAccessException">Si le claim userId n'est pas présent ou invalide</exception>
        private int GetClientIdFromClaims()
        {
            var claim = User.FindFirst("userId");

            if (claim == null)
            {
                _logger.LogWarning("Claim 'userId' manquant dans le token pour l'utilisateur {User}", User.Identity?.Name ?? "anonyme");
                throw new UnauthorizedAccessException("Token invalide ou utilisateur non identifié.");
            }

            if (!int.TryParse(claim.Value, out int clientId) || clientId <= 0)
            {
                _logger.LogWarning("Claim 'userId' invalide (non numérique ou ≤ 0) : {Value}", claim.Value);
                throw new UnauthorizedAccessException("Token invalide ou utilisateur non identifié.");
            }

            return clientId;
        }
    }
}