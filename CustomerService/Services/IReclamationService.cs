// Services/IReclamationService.cs
using CustomerService.Models;


namespace CustomerService.Services
{
    public interface IReclamationService
    {
        Task<Reclamation> CreateReclamationAsync(string userId, CreateReclamationDto dto);
        Task<List<ReclamationDto>> GetMyReclamationsAsync(string userId);
        Task<List<ReclamationDto>> GetAllReclamationsAsync(); // Pour ResponsableSAV
        Task<Reclamation?> GetReclamationByIdAsync(int reclamationId);
        Task<bool> UpdateReclamationStatusAsync(int reclamationId, ReclamationStatus status);
        Task<Reclamation?> UpdateReclamationAsync(int id, Reclamation reclamation);
    }
}