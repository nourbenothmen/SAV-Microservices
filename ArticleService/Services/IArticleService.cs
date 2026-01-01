using ArticleService.Models;
using ArticleService.Models.DTO;

namespace ArticleService.Services
{
    public interface IArticleService
    {
        // Gestion des Articles
        Task<IEnumerable<Article>> GetAllArticlesAsync();
        Task<Article?> GetArticleByIdAsync(int id);
        Task<Article?> GetArticleByReferenceAsync(string reference);
        Task<IEnumerable<Article>> GetArticlesByCategorieAsync(string categorie);
        Task<IEnumerable<Article>> GetArticlesByTypeAsync(string type);
        Task<IEnumerable<Article>> GetArticlesByMarqueAsync(string marque);
        Task<IEnumerable<Article>> SearchArticlesAsync(string searchTerm);
        Task<Article> CreateArticleAsync(Article article);
        Task<Article?> UpdateArticleAsync(int id, Article article);
        Task<bool> DeleteArticleAsync(int id);
        Task<bool> UpdateStockAsync(int id, int quantity);

        // Gestion des Articles Clients
        Task<IEnumerable<CustomerArticle>> GetAllCustomerArticlesAsync();
        Task<CustomerArticle?> GetCustomerArticleByIdAsync(int id);
        Task<IEnumerable<CustomerArticle>> GetCustomerArticlesByClientIdAsync(int clientId);
        Task<IEnumerable<CustomerArticle>> GetCustomerArticlesByArticleIdAsync(int articleId);
        Task<CustomerArticle?> GetCustomerArticleBySerialNumberAsync(string numeroSerie);
        Task<IEnumerable<CustomerArticle>> GetCustomerArticlesSousGarantieAsync(int clientId);
        Task<CustomerArticle> CreateCustomerArticleAsync(CustomerArticle customerArticle);
        Task<CustomerArticle?> UpdateCustomerArticleAsync(int id, CustomerArticle customerArticle);
        Task<bool> DeleteCustomerArticleAsync(int id);
        Task<bool> VerifierGarantieAsync(int customerArticleId);
        Task<List<MyArticleDto>> GetMyArticlesAsync(int clientId);
    }
}