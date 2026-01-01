namespace ArticleService.Models.DTO
{
    public class MyArticleDto
    {
        public int CustomerArticleId { get; set; }     // ID du lien client-article
        public int ArticleId { get; set; }             // ID de l'article catalogue
        public DateTime DateAchat { get; set; }   // ✅ AJOUT
        public string DisplayName { get; set; } = string.Empty; // "Atlantic - Chaudière XZ123"
        public string SerialNumber { get; set; } = string.Empty;
        public bool EstSousGarantie { get; set; }
        public DateTime? DateFinGarantie { get; set; }
    }
}
