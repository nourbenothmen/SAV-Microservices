using System.ComponentModel.DataAnnotations;

namespace CustomerService.Models
{
    public class Reclamation
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? SerialNumber { get; set; }  // Numéro de série article
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public ReclamationStatus Status { get; set; } = ReclamationStatus.EnAttente;

        // Navigation
        public Customer Customer { get; set; } = null!;
        public DateTime? ResolvedAt { get; set; }

        public int ArticleId { get; internal set; }
        public int? InterventionId { get; internal set; }
        public string? ProblemType { get; set; }                    // nouveau
        public DateTime? DesiredInterventionDate { get; set; }
    }

}
