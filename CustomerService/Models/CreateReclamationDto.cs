namespace CustomerService.Models
{
   
        public class CreateReclamationDto
        {
            public int ArticleId { get; set; }
            public string ProblemType { get; set; } = string.Empty; // Nouveau champ
            public string Description { get; set; } = string.Empty;
            public DateTime? DesiredInterventionDate { get; set; } // Nouveau champ (optionnel)
        }
    }
