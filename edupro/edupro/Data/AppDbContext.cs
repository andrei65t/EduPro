using Microsoft.EntityFrameworkCore;
using EduPro.Models;

namespace EduPro.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor - primește opțiunile de la dependency injection
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Tabelul pentru notițe
        public DbSet<Note> Notes { get; set; }

        // Tabelul pentru categorii
        public DbSet<Category> Categories { get; set; }

        // Configurare suplimentară pentru tabel
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Adaugă index pe CreatedAt pentru sortări rapide
            modelBuilder.Entity<Note>()
                .HasIndex(n => n.CreatedAt);

            // Configurare relație Note -> Category
            modelBuilder.Entity<Note>()
                .HasOne(n => n.Category)
                .WithMany(c => c.Notes)
                .HasForeignKey(n => n.CategoryId)
                .OnDelete(DeleteBehavior.SetNull); // Când ștergem o categorie, notițele rămân dar CategoryId devine null

            // Index pe numele categoriei pentru căutări rapide
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique(); // Numele categoriei trebuie să fie unic

            // Index pe CategoryId pentru filtrări rapide
            modelBuilder.Entity<Note>()
                .HasIndex(n => n.CategoryId);
        }
    }
}
