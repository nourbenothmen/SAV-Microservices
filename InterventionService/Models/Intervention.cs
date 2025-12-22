using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterventionService.Models
{
    public class Intervention
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReclamationId { get; set; }

        [Required]
        public int ClientId { get; set; }

        [Required]
        public int ArticleId { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime DateIntervention { get; set; }

        [Required]
        [StringLength(50)]
        public string Statut { get; set; } = "Planifiée"; // Planifiée, EnCours, Terminée, Annulée

        public bool EstSousGarantie { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantMainOeuvre { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantTotal { get; set; }

        [StringLength(500)]
        public string? Commentaire { get; set; }

        [Required]
        [StringLength(100)]
        public string TechnicienNom { get; set; } = string.Empty;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateMiseAJour { get; set; }

        // Navigation property
        public virtual ICollection<InterventionPart> Pieces { get; set; } = new List<InterventionPart>();
    }
}