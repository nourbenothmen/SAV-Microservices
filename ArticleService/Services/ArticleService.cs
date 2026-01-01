using ArticleService.Data;
using ArticleService.Models;
using ArticleService.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.Services
{
    public class ArticleServiceImpl : IArticleService
    {
        private readonly ArticleDbContext _context;
        private readonly ILogger<ArticleServiceImpl> _logger;

        public ArticleServiceImpl(ArticleDbContext context, ILogger<ArticleServiceImpl> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Gestion des Articles

        public async Task<IEnumerable<Article>> GetAllArticlesAsync()
        {
            try
            {
                return await _context.Articles
                    .OrderBy(a => a.Categorie)
                    .ThenBy(a => a.Nom)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des articles");
                throw;
            }
        }

        public async Task<Article?> GetArticleByIdAsync(int id)
        {
            try
            {
                return await _context.Articles
                    .Include(a => a.ArticlesClients)
                    .FirstOrDefaultAsync(a => a.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'article {Id}", id);
                throw;
            }
        }

        public async Task<Article?> GetArticleByReferenceAsync(string reference)
        {
            try
            {
                return await _context.Articles
                    .FirstOrDefaultAsync(a => a.Reference == reference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'article par référence {Reference}", reference);
                throw;
            }
        }

        public async Task<IEnumerable<Article>> GetArticlesByCategorieAsync(string categorie)
        {
            try
            {
                return await _context.Articles
                    .Where(a => a.Categorie == categorie)
                    .OrderBy(a => a.Nom)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des articles par catégorie {Categorie}", categorie);
                throw;
            }
        }

        public async Task<IEnumerable<Article>> GetArticlesByTypeAsync(string type)
        {
            try
            {
                return await _context.Articles
                    .Where(a => a.Type == type)
                    .OrderBy(a => a.Nom)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des articles par type {Type}", type);
                throw;
            }
        }

        public async Task<IEnumerable<Article>> GetArticlesByMarqueAsync(string marque)
        {
            try
            {
                return await _context.Articles
                    .Where(a => a.Marque == marque)
                    .OrderBy(a => a.Nom)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des articles par marque {Marque}", marque);
                throw;
            }
        }

        public async Task<IEnumerable<Article>> SearchArticlesAsync(string searchTerm)
        {
            try
            {
                var search = searchTerm.ToLower();
                return await _context.Articles
                    .Where(a => a.Nom.ToLower().Contains(search) ||
                               a.Reference.ToLower().Contains(search) ||
                               (a.Description != null && a.Description.ToLower().Contains(search)) ||
                               a.Marque.ToLower().Contains(search))
                    .OrderBy(a => a.Nom)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche d'articles avec le terme {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<Article> CreateArticleAsync(Article article)
        {
            try
            {
                article.DateCreation = DateTime.Now;
                _context.Articles.Add(article);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Article créé avec succès: {Id}", article.Id);
                return article;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'article");
                throw;
            }
        }

        public async Task<Article?> UpdateArticleAsync(int id, Article article)
        {
            try
            {
                var existingArticle = await _context.Articles.FindAsync(id);
                if (existingArticle == null)
                {
                    return null;
                }

                existingArticle.Nom = article.Nom;
                existingArticle.Reference = article.Reference;
                existingArticle.Description = article.Description;
                existingArticle.Categorie = article.Categorie;
                existingArticle.Type = article.Type;
                existingArticle.Marque = article.Marque;
                existingArticle.Modele = article.Modele;
                existingArticle.Prix = article.Prix;
                existingArticle.DureeGarantie = article.DureeGarantie;
                existingArticle.EstDisponible = article.EstDisponible;
                existingArticle.Stock = article.Stock;
                existingArticle.ImageUrl = article.ImageUrl;
                existingArticle.DateMiseAJour = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Article mis à jour avec succès: {Id}", id);
                return existingArticle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de l'article {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteArticleAsync(int id)
        {
            try
            {
                var article = await _context.Articles.FindAsync(id);
                if (article == null)
                {
                    return false;
                }

                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Article supprimé avec succès: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'article {Id}", id);
                throw;
            }
        }

        public async Task<bool> UpdateStockAsync(int id, int quantity)
        {
            try
            {
                var article = await _context.Articles.FindAsync(id);
                if (article == null)
                {
                    return false;
                }

                article.Stock += quantity;
                article.DateMiseAJour = DateTime.Now;

                if (article.Stock <= 0)
                {
                    article.EstDisponible = false;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Stock de l'article {Id} mis à jour: {Quantity}", id, quantity);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du stock de l'article {Id}", id);
                throw;
            }
        }

        #endregion

        #region Gestion des Articles Clients

        public async Task<IEnumerable<CustomerArticle>> GetAllCustomerArticlesAsync()
        {
            try
            {
                return await _context.CustomerArticles
                    .Include(ca => ca.Article)
                    .OrderByDescending(ca => ca.DateAchat)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des articles clients");
                throw;
            }
        }

        public async Task<CustomerArticle?> GetCustomerArticleByIdAsync(int id)
        {
            try
            {
                return await _context.CustomerArticles
                    .Include(ca => ca.Article)
                    .FirstOrDefaultAsync(ca => ca.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'article client {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<CustomerArticle>> GetCustomerArticlesByClientIdAsync(int clientId)
        {
            try
            {
                return await _context.CustomerArticles
                    .Include(ca => ca.Article)
                    .Where(ca => ca.ClientId == clientId)
                    .OrderByDescending(ca => ca.DateAchat)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des articles du client {ClientId}", clientId);
                throw;
            }
        }

        public async Task<IEnumerable<CustomerArticle>> GetCustomerArticlesByArticleIdAsync(int articleId)
        {
            try
            {
                return await _context.CustomerArticles
                    .Include(ca => ca.Article)
                    .Where(ca => ca.ArticleId == articleId)
                    .OrderByDescending(ca => ca.DateAchat)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des clients ayant l'article {ArticleId}", articleId);
                throw;
            }
        }

        public async Task<CustomerArticle?> GetCustomerArticleBySerialNumberAsync(string numeroSerie)
        {
            try
            {
                return await _context.CustomerArticles
                    .Include(ca => ca.Article)
                    .FirstOrDefaultAsync(ca => ca.NumeroSerie == numeroSerie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'article client par numéro de série {NumeroSerie}", numeroSerie);
                throw;
            }
        }

        public async Task<IEnumerable<CustomerArticle>> GetCustomerArticlesSousGarantieAsync(int clientId)
        {
            try
            {
                return await _context.CustomerArticles
                    .Include(ca => ca.Article)
                    .Where(ca => ca.ClientId == clientId && ca.EstSousGarantie)
                    .OrderByDescending(ca => ca.DateFinGarantie)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des articles sous garantie du client {ClientId}", clientId);
                throw;
            }
        }

        public async Task<CustomerArticle> CreateCustomerArticleAsync(CustomerArticle customerArticle)
        {
            try
            {
                // Récupérer l'article pour obtenir la durée de garantie
                var article = await _context.Articles.FindAsync(customerArticle.ArticleId);
                if (article == null)
                {
                    throw new ArgumentException("Article non trouvé", nameof(customerArticle.ArticleId));
                }

                // Calculer la date de fin de garantie
                customerArticle.DateFinGarantie = customerArticle.DateAchat.AddMonths(article.DureeGarantie);
                customerArticle.EstSousGarantie = DateTime.Now <= customerArticle.DateFinGarantie;
                customerArticle.DateCreation = DateTime.Now;

                _context.CustomerArticles.Add(customerArticle);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Article client créé avec succès: {Id}", customerArticle.Id);
                return customerArticle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'article client");
                throw;
            }
        }

        public async Task<CustomerArticle?> UpdateCustomerArticleAsync(int id, CustomerArticle customerArticle)
        {
            try
            {
                var existingCustomerArticle = await _context.CustomerArticles.FindAsync(id);
                if (existingCustomerArticle == null)
                {
                    return null;
                }

                existingCustomerArticle.NumeroSerie = customerArticle.NumeroSerie;
                existingCustomerArticle.DateAchat = customerArticle.DateAchat;
                existingCustomerArticle.NumeroFacture = customerArticle.NumeroFacture;
                existingCustomerArticle.Remarques = customerArticle.Remarques;
                existingCustomerArticle.DateMiseAJour = DateTime.Now;

                // Recalculer la date de fin de garantie si la date d'achat a changé
                var article = await _context.Articles.FindAsync(existingCustomerArticle.ArticleId);
                if (article != null)
                {
                    existingCustomerArticle.DateFinGarantie = existingCustomerArticle.DateAchat.AddMonths(article.DureeGarantie);
                    existingCustomerArticle.EstSousGarantie = DateTime.Now <= existingCustomerArticle.DateFinGarantie;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Article client mis à jour avec succès: {Id}", id);
                return existingCustomerArticle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de l'article client {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCustomerArticleAsync(int id)
        {
            try
            {
                var customerArticle = await _context.CustomerArticles.FindAsync(id);
                if (customerArticle == null)
                {
                    return false;
                }

                _context.CustomerArticles.Remove(customerArticle);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Article client supprimé avec succès: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'article client {Id}", id);
                throw;
            }
        }

        public async Task<bool> VerifierGarantieAsync(int customerArticleId)
        {
            try
            {
                var customerArticle = await _context.CustomerArticles.FindAsync(customerArticleId);
                if (customerArticle == null)
                {
                    return false;
                }

                var estSousGarantie = DateTime.Now <= customerArticle.DateFinGarantie;

                if (customerArticle.EstSousGarantie != estSousGarantie)
                {
                    customerArticle.EstSousGarantie = estSousGarantie;
                    customerArticle.DateMiseAJour = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                return estSousGarantie;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de la garantie {CustomerArticleId}", customerArticleId);
                throw;
            }



        }

        public async Task<List<MyArticleDto>> GetMyArticlesAsync(int clientId)
        {
            return await _context.CustomerArticles
                .Include(ca => ca.Article)
                .Where(ca => ca.ClientId == clientId)
                .Select(ca => new MyArticleDto
                {
                    CustomerArticleId = ca.Id,
                    ArticleId = ca.ArticleId,

                    SerialNumber = ca.NumeroSerie,
                    DateAchat = ca.DateAchat,                   // ✅ OK
                    DateFinGarantie = ca.DateFinGarantie,

                    EstSousGarantie = ca.DateFinGarantie > DateTime.Today,

                    // ✅ CONSTRUCTION DU NOM (PAS DisplayName dans Article)
                    DisplayName = ca.Article != null
                        ? ca.Article.Nom
                        : "Article inconnu"
                })
                .ToListAsync();
        }

       
        #endregion
    }
}