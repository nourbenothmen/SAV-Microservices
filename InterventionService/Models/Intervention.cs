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
        public decimal DureeIntervention { get; set; } = 0; // Nouveau: Heures d'intervention
        [Column(TypeName = "decimal(18,2)")]
        public decimal TarifHoraire { get; set; } = 40; // Nouveau: Tarif horaire par défaut
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantMainOeuvre { get; set; } // = Duree * TarifHoraire
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantDeplacement { get; set; } = 15; // Nouveau: Déplacement par défaut
        [Column(TypeName = "decimal(18,2)")]
        public decimal TauxTVA { get; set; } = 0.19m; // Nouveau: 19% par défaut
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantTotal { get; set; } // = (Pièces + MainOeuvre + Déplacement) * (1 + TVA)
        [StringLength(50)]
        public string? ModePaiement { get; set; } // Nouveau: Espèces, Carte, etc.
        [StringLength(50)]
        public string? StatutPaiement { get; set; } // Nouveau: Payé, EnAttente, etc.
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