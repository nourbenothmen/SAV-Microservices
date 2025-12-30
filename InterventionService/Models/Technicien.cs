namespace InterventionService.Models
{
    using System.ComponentModel.DataAnnotations;

    namespace InterventionService.Models
    {
        public class Technicien
        {
            [Key]
            public int Id { get; set; }

            [Required]
            [MaxLength(100)]
            public string Nom { get; set; }

            [Required]
            [MaxLength(100)]
            public string Prenom { get; set; }

            [MaxLength(50)]
            public string Telephone { get; set; }

            [MaxLength(100)]
            public string Specialite { get; set; }
        }
    }

}
