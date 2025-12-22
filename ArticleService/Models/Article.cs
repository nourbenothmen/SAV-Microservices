using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArticleService.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Reference { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Categorie { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Marque { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Modele { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Prix { get; set; }

        [Required]
        public int DureeGarantie { get; set; }

        public bool EstDisponible { get; set; } = true;

        [Required]
        public int Stock { get; set; }

        [StringLength(200)]
        public string? ImageUrl { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateMiseAJour { get; set; }

        public virtual ICollection<CustomerArticle> ArticlesClients { get; set; } = new List<CustomerArticle>();
    }
}