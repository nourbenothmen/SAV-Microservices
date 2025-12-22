using Microsoft.EntityFrameworkCore;
using InterventionService.Models;

namespace InterventionService.Data
{
    public class InterventionDbContext : DbContext
    {
        // Constructeur avec options (pour l'application)
        public InterventionDbContext(DbContextOptions<InterventionDbContext> options)
            : base(options)
        {
        }

        // Constructeur sans paramètres (pour les migrations)
        public InterventionDbContext()
        {
        }

        public DbSet<Intervention> Interventions { get; set; }
        public DbSet<InterventionPart> InterventionParts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Configuration pour les migrations
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=InterventionServiceDb;Trusted_Connection=True;MultipleActiveResultSets=true");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de l'entité Intervention
            modelBuilder.Entity<Intervention>(entity =>
            {
                entity.ToTable("Interventions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasIndex(e => e.ReclamationId);
                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => e.ArticleId);
                entity.HasIndex(e => e.DateIntervention);

                entity.Property(e => e.MontantMainOeuvre).HasPrecision(18, 2);
                entity.Property(e => e.MontantTotal).HasPrecision(18, 2);
            });

            // Configuration de l'entité InterventionPart
            modelBuilder.Entity<InterventionPart>(entity =>
            {
                entity.ToTable("InterventionParts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasIndex(e => e.InterventionId);

                entity.Property(e => e.PrixUnitaire).HasPrecision(18, 2);
                entity.Property(e => e.PrixTotal).HasPrecision(18, 2);

                // Relation avec Intervention
                entity.HasOne(e => e.Intervention)
                    .WithMany(i => i.Pieces)
                    .HasForeignKey(e => e.InterventionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}