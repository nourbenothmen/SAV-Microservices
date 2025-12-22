using InterventionService.Models;

namespace InterventionService.Services
{
    public interface IInterventionService
    {
        Task<IEnumerable<Intervention>> GetAllInterventionsAsync();
        Task<Intervention?> GetInterventionByIdAsync(int id);
        Task<IEnumerable<Intervention>> GetInterventionsByClientIdAsync(int clientId);
        Task<IEnumerable<Intervention>> GetInterventionsByReclamationIdAsync(int reclamationId);
        Task<Intervention> CreateInterventionAsync(Intervention intervention);
        Task<Intervention?> UpdateInterventionAsync(int id, Intervention intervention);
        Task<bool> DeleteInterventionAsync(int id);
        Task<InterventionPart> AddPartToInterventionAsync(int interventionId, InterventionPart part);
        Task<bool> RemovePartFromInterventionAsync(int interventionId, int partId);
        Task<decimal> CalculateTotalCostAsync(int interventionId);
    }
}