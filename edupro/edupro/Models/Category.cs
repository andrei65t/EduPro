using System.ComponentModel.DataAnnotations;

namespace EduPro.Models
{
    public class Category
    {
        // ID-ul categoriei (cheie primară, auto-increment)
        public int Id { get; set; }

        // Numele categoriei (OBLIGATORIU și UNIC)
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // Culoare pentru categorie (opțional, pentru UI)
        [MaxLength(7)] // Format: #RRGGBB
        public string? Color { get; set; }

        // Data creării
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relație: o categorie poate avea mai multe notițe
        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
