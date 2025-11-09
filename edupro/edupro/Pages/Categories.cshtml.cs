using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EduPro.Data;
using EduPro.Models;

namespace EduPro.Pages
{
    public class CategoriesModel : PageModel
    {
        private readonly AppDbContext _dbContext;

        public List<CategoryViewModel> Categories { get; set; } = new();

        [BindProperty]
        public string? NewCategoryName { get; set; }

        [BindProperty]
        public string? NewCategoryColor { get; set; }

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public string? MessageType { get; set; } // "success" sau "error"

        public CategoriesModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task OnGetAsync()
        {
            await LoadCategoriesAsync();
        }

        // Adaugă categorie nouă
        public async Task<IActionResult> OnPostAddCategoryAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName))
            {
                Message = "Numele categoriei nu poate fi gol!";
                MessageType = "error";
                await LoadCategoriesAsync();
                return Page();
            }

            // Verifică dacă categoria există deja
            var existingCategory = await _dbContext.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == NewCategoryName.Trim().ToLower());

            if (existingCategory != null)
            {
                Message = "O categorie cu acest nume există deja!";
                MessageType = "error";
                await LoadCategoriesAsync();
                return Page();
            }

            // Creează categoria nouă
            var category = new Category
            {
                Name = NewCategoryName.Trim(),
                Color = string.IsNullOrWhiteSpace(NewCategoryColor) ? "#6366f1" : NewCategoryColor
            };

            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();

            Message = $"Categoria '{category.Name}' a fost creată cu succes!";
            MessageType = "success";

            return RedirectToPage();
        }

        // Șterge o categorie
        public async Task<IActionResult> OnPostDeleteCategoryAsync(int categoryId)
        {
            var category = await _dbContext.Categories
                .Include(c => c.Notes)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
            {
                Message = "Categoria nu a fost găsită!";
                MessageType = "error";
                return RedirectToPage();
            }

            var notesCount = category.Notes.Count;

            // Șterge categoria (notițele vor avea CategoryId = null datorită OnDelete(SetNull))
            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();

            Message = $"Categoria '{category.Name}' a fost ștearsă! ({notesCount} {(notesCount == 1 ? "notiță" : "notițe")} {(notesCount == 1 ? "mutată" : "mutate")} în 'Fără categorie')";
            MessageType = "success";

            return RedirectToPage();
        }

        // Mută notițele dintr-o categorie în alta
        public async Task<IActionResult> OnPostMoveCategoryAsync(int fromCategoryId, int? toCategoryId)
        {
            var notes = await _dbContext.Notes
                .Where(n => n.CategoryId == fromCategoryId)
                .ToListAsync();

            if (!notes.Any())
            {
                Message = "Nu există notițe de mutat!";
                MessageType = "error";
                return RedirectToPage();
            }

            // Actualizează categoria pentru toate notițele
            foreach (var note in notes)
            {
                note.CategoryId = toCategoryId;
                note.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            var fromCategory = await _dbContext.Categories.FindAsync(fromCategoryId);
            var toCategory = toCategoryId.HasValue 
                ? await _dbContext.Categories.FindAsync(toCategoryId.Value) 
                : null;

            var fromName = fromCategory?.Name ?? "Necunoscut";
            var toName = toCategory?.Name ?? "Fără categorie";

            Message = $"{notes.Count} {(notes.Count == 1 ? "notiță mutată" : "notițe mutate")} din '{fromName}' în '{toName}'!";
            MessageType = "success";

            return RedirectToPage();
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await _dbContext.Categories
                .Include(c => c.Notes)
                .OrderBy(c => c.Name)
                .ToListAsync();

            Categories = categories.Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Color = c.Color ?? "#6366f1",
                NotesCount = c.Notes.Count,
                CreatedAt = c.CreatedAt
            }).ToList();
        }

        public class CategoryViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Color { get; set; } = string.Empty;
            public int NotesCount { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
