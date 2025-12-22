using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ArticleService.Models;
using ArticleService.Services;

namespace ArticleService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Ajout de l'autorisation globale
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;
        private readonly ILogger<ArticlesController> _logger;

        public ArticlesController(IArticleService articleService, ILogger<ArticlesController> logger)
        {
            _articleService = articleService;
            _logger = logger;
        }

        /// <summary>
        /// Récupère tous les articles
        /// GET: api/Articles
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<Article>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Article>>> GetAllArticles()
        {
            try
            {
                var articles = await _articleService.GetAllArticlesAsync();
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des articles");
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération des articles", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère un article par son ID
        /// GET: api/Articles/5
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Article), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Article>> GetArticleById(int id)
        {
            try
            {
                var article = await _articleService.GetArticleByIdAsync(id);
                if (article == null)
                {
                    return NotFound(new { message = $"Article avec l'ID {id} non trouvé" });
                }
                return Ok(article);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'article {Id}", id);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération de l'article", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère un article par sa référence
        /// GET: api/Articles/reference/ROB-LAV-001
        /// </summary>
        [HttpGet("reference/{reference}")]
        [AllowAnonymous] // Accessible sans authentification
        [ProducesResponseType(typeof(Article), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Article>> GetArticleByReference(string reference)
        {
            try
            {
                var article = await _articleService.GetArticleByReferenceAsync(reference);
                if (article == null)
                {
                    return NotFound(new { message = $"Article avec la référence {reference} non trouvé" });
                }
                return Ok(article);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'article par référence {Reference}", reference);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération de l'article", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère les articles par catégorie
        /// GET: api/Articles/categorie/Sanitaire
        /// </summary>
        [HttpGet("categorie/{categorie}")]
        [AllowAnonymous] // Accessible sans authentification
        [ProducesResponseType(typeof(IEnumerable<Article>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Article>>> GetArticlesByCategorie(string categorie)
        {
            try
            {
                var articles = await _articleService.GetArticlesByCategorieAsync(categorie);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des articles par catégorie {Categorie}", categorie);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération des articles", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère les articles par type
        /// GET: api/Articles/type/Robinetterie
        /// </summary>
        [HttpGet("type/{type}")]
        [AllowAnonymous] // Accessible sans authentification
        [ProducesResponseType(typeof(IEnumerable<Article>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Article>>> GetArticlesByType(string type)
        {
            try
            {
                var articles = await _articleService.GetArticlesByTypeAsync(type);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des articles par type {Type}", type);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération des articles", error = ex.Message });
            }
        }

        /// <summary>
        /// Récupère les articles par marque
        /// GET: api/Articles/marque/Grohe
        /// </summary>
        [HttpGet("marque/{marque}")]
        [AllowAnonymous] // Accessible sans authentification
        [ProducesResponseType(typeof(IEnumerable<Article>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Article>>> GetArticlesByMarque(string marque)
        {
            try
            {
                var articles = await _articleService.GetArticlesByMarqueAsync(marque);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des articles par marque {Marque}", marque);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération des articles", error = ex.Message });
            }
        }

        /// <summary>
        /// Recherche des articles
        /// GET: api/Articles/search/robinet
        /// </summary>
        [HttpGet("search/{searchTerm}")]
        [AllowAnonymous] // Accessible sans authentification
        [ProducesResponseType(typeof(IEnumerable<Article>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Article>>> SearchArticles(string searchTerm)
        {
            try
            {
                var articles = await _articleService.SearchArticlesAsync(searchTerm);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche d'articles avec le terme {SearchTerm}", searchTerm);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la recherche des articles", error = ex.Message });
            }
        }

        /// <summary>
        /// Crée un nouvel article
        /// POST: api/Articles
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "ResponsableSAV")]
        [ProducesResponseType(typeof(Article), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Article>> CreateArticle([FromBody] Article article)
        {
            try
            {
                var roles = User.Claims.Where(c => c.Type == "role").Select(c => c.Value);
                _logger.LogInformation("Roles dans le token: {Roles}", string.Join(",", roles));

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdArticle = await _articleService.CreateArticleAsync(article);
                return CreatedAtAction(nameof(GetArticleById), new { id = createdArticle.Id }, createdArticle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'article");
                return StatusCode(500, new { message = "Une erreur est survenue lors de la création de l'article", error = ex.Message });
            }
        }

        /// <summary>
        /// Met à jour un article existant
        /// PUT: api/Articles/5
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "ResponsableSAV")]
        [ProducesResponseType(typeof(Article), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Article>> UpdateArticle(int id, [FromBody] Article article)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedArticle = await _articleService.UpdateArticleAsync(id, article);
                if (updatedArticle == null)
                {
                    return NotFound(new { message = $"Article avec l'ID {id} non trouvé" });
                }

                return Ok(updatedArticle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de l'article {Id}", id);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la mise à jour de l'article", error = ex.Message });
            }
        }

        /// <summary>
        /// Supprime un article
        /// DELETE: api/Articles/5
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ResponsableSAV")] 
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteArticle(int id)
        {
            try
            {
                var result = await _articleService.DeleteArticleAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Article avec l'ID {id} non trouvé" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'article {Id}", id);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la suppression de l'article", error = ex.Message });
            }
        }

        /// <summary>
        /// Met à jour le stock d'un article
        /// PATCH: api/Articles/5/stock
        /// Body: 10 (quantité à ajouter ou retirer)
        /// </summary>
        [HttpPatch("{id}/stock")]
        [Authorize(Roles = "ResponsableSAV")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateStock(int id, [FromBody] int quantity)
        {
            try
            {
                var result = await _articleService.UpdateStockAsync(id, quantity);
                if (!result)
                {
                    return NotFound(new { message = $"Article avec l'ID {id} non trouvé" });
                }

                return Ok(new { message = "Stock mis à jour avec succès", articleId = id, quantityChanged = quantity });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du stock de l'article {Id}", id);
                return StatusCode(500, new { message = "Une erreur est survenue lors de la mise à jour du stock", error = ex.Message });
            }
        }
    }
}