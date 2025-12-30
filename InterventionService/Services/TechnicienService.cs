using InterventionService.Data;
using InterventionService.Models.InterventionService.Models;
using Microsoft.EntityFrameworkCore;

namespace InterventionService.Services
{
    public class TechnicienService
    {
        private readonly InterventionDbContext _context;

        public TechnicienService(InterventionDbContext context)
        {
            _context = context;
        }

        // Créer un technicien
        public async Task<Technicien> CreateTechnicienAsync(Technicien technicien)
        {
            _context.Techniciens.Add(technicien);
            await _context.SaveChangesAsync();
            return technicien;
        }

        // Obtenir tous les techniciens
        public async Task<List<Technicien>> GetAllTechniciensAsync()
        {
            return await _context.Techniciens.ToListAsync();
        }

        // Obtenir un technicien par Id
        public async Task<Technicien> GetTechnicienByIdAsync(int id)
        {
            return await _context.Techniciens.FindAsync(id);
        }

        // Mettre à jour un technicien
        public async Task<Technicien> UpdateTechnicienAsync(Technicien technicien)
        {
            _context.Entry(technicien).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return technicien;
        }

        // Supprimer un technicien
        public async Task<bool> DeleteTechnicienAsync(int id)
        {
            var technicien = await _context.Techniciens.FindAsync(id);
            if (technicien == null) return false;

            _context.Techniciens.Remove(technicien);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
