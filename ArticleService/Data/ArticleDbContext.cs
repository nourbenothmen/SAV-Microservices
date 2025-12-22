using Microsoft.EntityFrameworkCore;
using ArticleService.Models;

namespace ArticleService.Data
{
    public class ArticleDbContext : DbContext
    {
        public ArticleDbContext(DbContextOptions<ArticleDbContext> options)
            : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<CustomerArticle> CustomerArticles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de l'entité Article
            modelBuilder.Entity<Article>(entity =>
            {
                entity.ToTable("Articles");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasIndex(e => e.Reference).IsUnique();
                entity.HasIndex(e => e.Categorie);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.Marque);

                entity.Property(e => e.Prix).HasPrecision(18, 2);
            });

            // Configuration de l'entité CustomerArticle
            modelBuilder.Entity<CustomerArticle>(entity =>
            {
                entity.ToTable("CustomerArticles");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => e.ArticleId);
                entity.HasIndex(e => e.NumeroSerie).IsUnique();
                entity.HasIndex(e => e.DateAchat);

                // Relation avec Article
                entity.HasOne(e => e.Article)
                    .WithMany(a => a.ArticlesClients)
                    .HasForeignKey(e => e.ArticleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Données de seed pour les articles
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Article>().HasData(
                // Articles Sanitaire
                new Article
                {
                    Id = 1,
                    Nom = "Robinet Mitigeur Lavabo",
                    Reference = "ROB-LAV-001",
                    Description = "Mitigeur chromé pour lavabo avec économiseur d'eau",
                    Categorie = "Sanitaire",
                    Type = "Robinetterie",
                    Marque = "Grohe",
                    Modele = "Eurosmart",
                    Prix = 89.99m,
                    DureeGarantie = 60,
                    EstDisponible = true,
                    Stock = 25,
                    DateCreation = new DateTime(2024, 1, 1)
                },
                new Article
                {
                    Id = 2,
                    Nom = "Robinet Mitigeur Douche",
                    Reference = "ROB-DOU-001",
                    Description = "Mitigeur thermostatique pour douche",
                    Categorie = "Sanitaire",
                    Type = "Robinetterie",
                    Marque = "Hansgrohe",
                    Modele = "ShowerSelect",
                    Prix = 245.00m,
                    DureeGarantie = 60,
                    EstDisponible = true,
                    Stock = 15,
                    DateCreation = new DateTime(2024, 1, 1)
                },
                new Article
                {
                    Id = 3,
                    Nom = "WC Suspendu Compact",
                    Reference = "WC-SUS-001",
                    Description = "WC suspendu avec chasse d'eau économique",
                    Categorie = "Sanitaire",
                    Type = "WC",
                    Marque = "Geberit",
                    Modele = "Rimfree",
                    Prix = 320.00m,
                    DureeGarantie = 24,
                    EstDisponible = true,
                    Stock = 10,
                    DateCreation = new DateTime(2024, 1, 1)
                },
                new Article
                {
                    Id = 4,
                    Nom = "Lavabo en Céramique",
                    Reference = "LAV-CER-001",
                    Description = "Lavabo rectangulaire 60x45cm",
                    Categorie = "Sanitaire",
                    Type = "Lavabo",
                    Marque = "Ideal Standard",
                    Modele = "Connect",
                    Prix = 125.00m,
                    DureeGarantie = 24,
                    EstDisponible = true,
                    Stock = 20,
                    DateCreation = new DateTime(2024, 1, 1)
                },
                new Article
                {
                    Id = 5,
                    Nom = "Baignoire Acrylique",
                    Reference = "BAI-ACR-001",
                    Description = "Baignoire rectangulaire 170x75cm",
                    Categorie = "Sanitaire",
                    Type = "Baignoire",
                    Marque = "Villeroy & Boch",
                    Modele = "Oberon",
                    Prix = 580.00m,
                    DureeGarantie = 120,
                    EstDisponible = true,
                    Stock = 8,
                    DateCreation = new DateTime(2024, 1, 1)
                },
                // Articles Chauffage
                new Article
                {
                    Id = 6,
                    Nom = "Chaudière Gaz Murale",
                    Reference = "CHAU-GAZ-001",
                    Description = "Chaudière à condensation 24kW",
                    Categorie = "Chauffage",
                    Type = "Chaudière",
                    Marque = "Viessmann",
                    Modele = "Vitodens 100-W",
                    Prix = 2850.00m,
                    DureeGarantie = 24,
                    EstDisponible = true,
                    Stock = 5,
                    DateCreation = new DateTime(2024, 1, 1)
                },
                new Article
                {
                    Id = 7,
                    Nom = "Chaudière Gaz Sol",
                    Reference = "CHAU-GAZ-002",
                    Description = "Chaudière au sol à condensation 30kW",
                    Categorie = "Chauffage",
                    Type = "Chaudière",
                    Marque = "De Dietrich",
                    Modele = "MCR-G 24/28 MI+",
                    Prix = 3450.00m,
                    DureeGarantie = 24,
                    EstDisponible = true,
                    Stock = 3,
                    DateCreation = new DateTime(2024, 1, 1)
                },
                new Article
                {
                    Id = 8,
                    Nom = "Radiateur Acier Panneau",
                    Reference = "RAD-ACR-001",
                    Description = "Radiateur acier type 22 - 600x1000mm",
                    Categorie = "Chauffage",
                    Type = "Radiateur",
                    Marque = "Thermor",
                    Modele = "Evidence",
                    Prix = 185.00m,
                    DureeGarantie = 120,
                    EstDisponible = true,
                    Stock = 30,
                    DateCreation = new DateTime(2024, 1, 1)
                },
                new Article
                {
                    Id = 9,
                    Nom = "Radiateur Aluminium Design",
                    Reference = "RAD-ALU-001",
                    Description = "Radiateur design vertical 1800mm",
                    Categorie = "Chauffage",
                    Type = "Radiateur",
                    Marque = "Acova",
                    Modele = "Fassane",
                    Prix = 425.00m,
                    DureeGarantie = 120,
                    EstDisponible = true,
                    Stock = 12,
                    DateCreation = new DateTime(2024, 1, 1)
                },
                new Article
                {
                    Id = 10,
                    Nom = "Thermostat Connecté",
                    Reference = "THER-CON-001",
                    Description = "Thermostat intelligent WiFi programmable",
                    Categorie = "Chauffage",
                    Type = "Thermostat",
                    Marque = "Nest",
                    Modele = "Learning Thermostat",
                    Prix = 249.00m,
                    DureeGarantie = 24,
                    EstDisponible = true,
                    Stock = 18,
                    DateCreation = new DateTime(2024, 1, 1)
                },
                new Article
                {
                    Id = 11,
                    Nom = "Vanne Thermostatique",
                    Reference = "VAN-THER-001",
                    Description = "Vanne thermostatique pour radiateur",
                    Categorie = "Chauffage",
                    Type = "Accessoire",
                    Marque = "Danfoss",
                    Modele = "RA-N",
                    Prix = 45.00m,
                    DureeGarantie = 60,
                    EstDisponible = true,
                    Stock = 50,
                    DateCreation = new DateTime(2024, 1, 1)
                },
                new Article
                {
                    Id = 12,
                    Nom = "Pompe à Chaleur Air-Eau",
                    Reference = "PAC-AIR-001",
                    Description = "Pompe à chaleur air-eau 8kW",
                    Categorie = "Chauffage",
                    Type = "Pompe à Chaleur",
                    Marque = "Daikin",
                    Modele = "Altherma 3",
                    Prix = 5200.00m,
                    DureeGarantie = 36,
                    EstDisponible = true,
                    Stock = 2,
                    DateCreation = new DateTime(2024, 1, 1)
                }
            );
        }
    }
}