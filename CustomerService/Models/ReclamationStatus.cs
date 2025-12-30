namespace CustomerService.Models
{
    public enum ReclamationStatus
    {
        EnAttente,    // Réclamation créée, pas encore d'intervention
        Planifiée,    // Intervention planifiée
        EnCours,      // Intervention en cours
        Terminée      // Intervention terminée → réclamation résolue
    }
}