using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace InterventionService.Models
{
    public class InterventionPart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InterventionId { get; set; }

        [Required]
        [StringLength(100)]
        public string NomPiece { get; set; } = string.Empty;

        
        [StringLength(50)]
        public string? Reference { get; set; } // ? = nullable, et suppression de [Required]

        [Required]
        public int Quantite { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrixUnitaire { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrixTotal { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        // Navigation property
        [ForeignKey("InterventionId")]
        [JsonIgnore]
        public virtual Intervention? Intervention { get; set; }
    }
}