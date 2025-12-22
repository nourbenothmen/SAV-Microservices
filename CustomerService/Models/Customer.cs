using System.ComponentModel.DataAnnotations;

namespace CustomerService.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string UserId { get; set; }         // Id provenant d'AuthService.Users
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation vers les réclamations
        public List<Reclamation> Reclamations { get; set; } = new();

    }
}
