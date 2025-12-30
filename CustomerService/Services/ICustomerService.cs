// CustomerService/Services/ICustomerService.cs
using CustomerService.Models;
using CustomerService.Models.DTOs;

namespace CustomerService.Services
{
    public interface ICustomerService
    {
        // Récupérer tous les clients (ResponsableSAV uniquement)
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();

        // Récupérer un client par son ID
        Task<CustomerDto?> GetCustomerByIdAsync(int id);

        // Récupérer un client par son UserId (pour /me)
        Task<CustomerDto?> GetCustomerByUserIdAsync(string userId);

        // Créer un nouveau client (ResponsableSAV ou lors de l'inscription)
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request);

        // Mettre à jour un client (client = seulement le sien, SAV = tous)
        Task<bool> UpdateCustomerAsync(int id, UpdateCustomerRequest request);

        // Supprimer un client (ResponsableSAV uniquement)
        Task<bool> DeleteCustomerAsync(int id);


    }
}