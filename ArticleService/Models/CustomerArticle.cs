using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArticleService.Models
{
    public class CustomerArticle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }

        [Required]
        public int ArticleId { get; set; }

        [Required]
        [StringLength(100)]
        public string NumeroSerie { get; set; } = string.Empty;

        [Required]
        public DateTime DateAchat { get; set; }

        public DateTime DateFinGarantie { get; set; }

        public bool EstSousGarantie { get; set; }

        [StringLength(100)]
        public string? NumeroFacture { get; set; }

        [StringLength(500)]
        public string? Remarques { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateMiseAJour { get; set; }

        [ForeignKey("ArticleId")]
        public virtual Article? Article { get; set; }
    }
}