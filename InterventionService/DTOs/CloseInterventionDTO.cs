namespace InterventionService.DTOs
{
    public class CloseInterventionDTO
    {
        public decimal DureeIntervention { get; set; }
        public decimal TarifHoraire { get; set; } = 40;
        public decimal MontantDeplacement { get; set; } = 15;
        public decimal TauxTVA { get; set; } = 0.19m;
        public string? ModePaiement { get; set; }
        public string? StatutPaiement { get; set; }
    }
}