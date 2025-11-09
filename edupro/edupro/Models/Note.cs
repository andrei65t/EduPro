using System.ComponentModel.DataAnnotations;

namespace EduPro.Models
{
    public class Note
    {
        // ID-ul notei (cheie primară, auto-increment)
        public int Id { get; set; }

        // Textul complet extras din imagine (OBLIGATORIU)
        [Required]
        public string ExtractedText { get; set; } = string.Empty;

        // Rezumatul generat automat (opțional)
        public string? Summary { get; set; }

        // Titlul dat de utilizator (opțional)
        public string? Title { get; set; }

        // Numele fișierului original (pentru referință)
        public string? OriginalFileName { get; set; }

        // Data și ora creării
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Data și ora ultimei modificări (pentru viitoare editări)
        public DateTime? UpdatedAt { get; set; }

        // --- SISTEM DE CATEGORII ---
        
        // Foreign Key către Category (poate fi null dacă nota nu are categorie)
        public int? CategoryId { get; set; }

        // Navigation property către Category
        public Category? Category { get; set; }
    }
}
