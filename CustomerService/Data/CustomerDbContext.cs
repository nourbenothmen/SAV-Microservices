using CustomerService.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Data
{
    public class CustomerDbContext : DbContext
    {
        public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Reclamation> Reclamations => Set<Reclamation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Index unique sur UserId
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.UserId)
                .IsUnique();

            // Enum → string pour Status
            modelBuilder.Entity<Reclamation>()
                .Property(r => r.Status)
                .HasConversion<string>();
        }
    }
}
