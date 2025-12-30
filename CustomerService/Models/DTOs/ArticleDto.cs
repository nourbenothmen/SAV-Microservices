namespace CustomerService.Models.DTOs
{
    public class ArticleDto
    {
        public int Id { get; set; }
        public string Marque { get; set; } = "";
        public string Nom { get; set; } = "";
        public string Modele { get; set; } = "";
    }
}
