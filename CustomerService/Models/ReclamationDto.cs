namespace CustomerService.Models
{
    public class ReclamationDto
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public string ArticleNom { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ReclamationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? SerialNumber { get; set; }
        public string? ProblemType { get; set; }
        public DateTime? DesiredInterventionDate { get; set; }
        public bool EstSousGarantie { get; set; }

        // 🔹 Ajout pour le client
        public int CustomerId { get; set; }
        public string CustomerNom { get; set; } = string.Empty;
    }


}
