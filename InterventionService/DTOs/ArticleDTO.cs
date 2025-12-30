using System.Text.Json.Serialization;

namespace InterventionService.DTOs
{
    public class ArticleDTO
    {
        public int Id { get; set; }

        [JsonPropertyName("nom")]
        public string Nom { get; set; }

        public string DisplayName => Nom; // Pour rester compatible avec ton code existant
    }
}
