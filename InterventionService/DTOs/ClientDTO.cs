using System.Text.Json.Serialization;

namespace InterventionService.DTOs
{
    public class ClientDTO
    {
        public int Id { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = string.Empty;

        public string NomComplet => $"{FirstName} {LastName}".Trim();

        // Ajoute ces propriétés si elles existent dans la réponse du microservice
        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("phone")]
        public string? Telephone { get; set; }

        // Ou si les noms sont différents, adapte-les :
        // public string? Email { get; set; }
        // public string? Telephone { get; set; }
    }
}