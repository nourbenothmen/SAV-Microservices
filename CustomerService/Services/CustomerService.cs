// CustomerService/Services/CustomerService.cs
using CustomerService.Data;
using CustomerService.Models;
using CustomerService.Models.DTOs;
using CustomerService.Services.Http;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly CustomerDbContext _context;
        private readonly AuthApiClient _authApiClient;


        public CustomerService(CustomerDbContext context, AuthApiClient authApiClient)
        {
            _context = context;
            _authApiClient = authApiClient;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            return await _context.Customers
      
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                   
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Phone = c.Phone,
                    Address = c.Address,
                    CreatedAt = c.CreatedAt
                })
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            var customer = await _context.Customers
                
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null) return null;

            return new CustomerDto
            {
                Id = customer.Id,
                UserId = customer.UserId,
                
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Phone = customer.Phone,
                Address = customer.Address,
                CreatedAt = customer.CreatedAt
            };
        }

        public async Task<CustomerDto?> GetCustomerByUserIdAsync(string userId)
        {
            var customer = await _context.Customers
                
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null) return null;
            return new CustomerDto
            {
                Id = customer.Id,
                UserId = customer.UserId,
                
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Phone = customer.Phone,
                Address = customer.Address,
                CreatedAt = customer.CreatedAt
            };
        }

        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request)
        {
            // Vérifier si un client existe déjà avec ce UserId
            var exists = await _context.Customers.AnyAsync(c => c.UserId == request.UserId);
            if (exists)
                throw new InvalidOperationException("Un client existe déjà pour cet utilisateur.");

            var customer = new Customer
            {
                UserId = request.UserId,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Phone = request.Phone.Trim(),
                Address = request.Address.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Charger l'email depuis AuthService (via la table User partagée ou appel API)
            //var userEmail = await _authApiClient.GetUserEmailAsync(request.UserId)
                  //?? "email@inconnu.com";


            return new CustomerDto
            {
                Id = customer.Id,
                UserId = customer.UserId,
                //Email = userEmail,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Phone = customer.Phone,
                Address = customer.Address,
                CreatedAt = customer.CreatedAt
            };
        }

        public async Task<bool> UpdateCustomerAsync(int id, UpdateCustomerRequest request)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return false;

            if (!string.IsNullOrWhiteSpace(request.FirstName))
                customer.FirstName = request.FirstName.Trim();

            if (!string.IsNullOrWhiteSpace(request.LastName))
                customer.LastName = request.LastName.Trim();

            if (!string.IsNullOrWhiteSpace(request.Phone))
                customer.Phone = request.Phone.Trim();

            if (!string.IsNullOrWhiteSpace(request.Address))
                customer.Address = request.Address.Trim();

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Reclamations)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null) return false;

            // Option : bloquer suppression si réclamations en cours
            if (customer.Reclamations.Any(r => r.Status == ReclamationStatus.EnAttente || r.Status == ReclamationStatus.EnCours))
                throw new InvalidOperationException("Impossible de supprimer un client avec des réclamations en cours.");

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }

       
    }
}