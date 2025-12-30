using ArticleService.Models.DTO;

namespace ArticleService.Models
{
    public class CustomerArticleResponse
    {
        public int Id { get; set; }
        public string NumeroSerie { get; set; } = string.Empty;
        public bool EstSousGarantie { get; set; }
        public ArticleDto Article { get; set; } = null!;
    }

    public class ArticleDto
    {
        public string Marque { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string Modele { get; set; } = string.Empty;
    }
}
