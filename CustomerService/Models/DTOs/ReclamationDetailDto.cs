// Dans CustomerService.Models.DTOs
public class ReclamationDetailsDto
{
    public int Id { get; set; }
    public string ArticleNom { get; set; } = "Article inconnu";
    public string Description { get; set; } = string.Empty;
    public string ProblemType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // string pour le frontend
    public DateTime CreatedAt { get; set; }
    public DateTime? DesiredInterventionDate { get; set; }
    public string TechnicienNom { get; set; } = "Non assigné";
    public DateTime? DateIntervention { get; set; }
    public bool EstSousGarantie { get; set; }
    public decimal? MontantTotal { get; set; } // seulement si hors garantie et Terminée
}