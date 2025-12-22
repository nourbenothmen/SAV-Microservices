// InterventionDTO.cs
using System.ComponentModel.DataAnnotations;

namespace InterventionService.DTOs
{
    public class InterventionDTO
    {
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
        public string Statut { get; set; } = "Planifiée";

        public bool EstSousGarantie { get; set; }

        public decimal MontantMainOeuvre { get; set; }

        public decimal MontantTotal { get; set; }

        [StringLength(500)]
        public string? Commentaire { get; set; }

        [Required]
        [StringLength(100)]
        public string TechnicienNom { get; set; } = string.Empty;

        public DateTime DateCreation { get; set; }

        public DateTime? DateMiseAJour { get; set; }

        public List<InterventionPartDTO> Pieces { get; set; } = new();
    }

    public class CreateInterventionDTO
    {
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
        public string Statut { get; set; } = "Planifiée";

        public bool EstSousGarantie { get; set; }

        public decimal MontantMainOeuvre { get; set; }

        [StringLength(500)]
        public string? Commentaire { get; set; }

        [Required]
        [StringLength(100)]
        public string TechnicienNom { get; set; } = string.Empty;

        public List<CreateInterventionPartDTO>? Pieces { get; set; }
    }

    public class InterventionPartDTO
    {
        public int Id { get; set; }

        public int InterventionId { get; set; }

        [Required]
        [StringLength(100)]
        public string NomPiece { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Reference { get; set; } = string.Empty;

        [Required]
        public int Quantite { get; set; }

        [Required]
        public decimal PrixUnitaire { get; set; }

        public decimal PrixTotal { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }
    }

    public class CreateInterventionPartDTO
    {
        [Required]
        [StringLength(100)]
        public string NomPiece { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Reference { get; set; } = string.Empty;

        [Required]
        public int Quantite { get; set; }

        [Required]
        public decimal PrixUnitaire { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }
    }
}