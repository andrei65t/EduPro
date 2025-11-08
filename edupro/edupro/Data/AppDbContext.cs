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

		// Configurare suplimentară pentru tabel
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Adaugă index pe CreatedAt pentru sortări rapide
			modelBuilder.Entity<Note>()
				.HasIndex(n => n.CreatedAt);

			// Poți adăuga alte configurări aici dacă e nevoie
		}
	}
}
