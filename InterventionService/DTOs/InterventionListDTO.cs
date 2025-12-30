namespace InterventionService.DTOs
{
    public class InterventionListDTO
    {
        public int Id { get; set; }

        public int ReclamationId { get; set; }         // ✅ Ajouter l’ID de la réclamation
        public int ClientId { get; set; }
        public string ClientNom { get; set; } = string.Empty;

        public int ArticleId { get; set; }
        public string ArticleNom { get; set; } = string.Empty;

        public bool EstSousGarantie { get; set; }     // ✅ Ajouter le bool pour savoir si l’article est sous garantie
        public string TechnicienNom { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;

        public DateTime DateCreation { get; set; }
    }
}
