using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EduPro.Data;
using EduPro.Models;

namespace EduPro.Pages
{
    public class NotesListModel : PageModel
    {
        private readonly AppDbContext _dbContext;

        public List<Note> Notes { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public int? SelectedCategoryId { get; set; }

        [BindProperty]
        public int NoteId { get; set; }

        [BindProperty]
        public int? NewCategoryId { get; set; }

        [BindProperty]
        public string? NewCategoryName { get; set; }

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public string? MessageType { get; set; }

        public string CurrentFilter => SelectedCategoryId.HasValue 
            ? Categories.FirstOrDefault(c => c.Id == SelectedCategoryId.Value)?.Name ?? "Necunoscut"
            : "Toate notițele";

        public NotesListModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        // Schimbă categoria unei notițe (cu autocreare dacă e nevoie)
        public async Task<IActionResult> OnPostChangeCategoryAsync()
        {
            var note = await _dbContext.Notes.FindAsync(NoteId);
            if (note == null)
            {
                Message = "Notița nu a fost găsită!";
                MessageType = "error";
                return RedirectToPage();
            }

            // Dacă utilizatorul a introdus un nume nou de categorie
            if (!string.IsNullOrWhiteSpace(NewCategoryName))
            {
                // Verifică dacă categoria există deja (case-insensitive)
                var existingCategory = await _dbContext.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == NewCategoryName.Trim().ToLower());

                if (existingCategory != null)
                {
                    // Folosește categoria existentă
                    note.CategoryId = existingCategory.Id;
                    Message = $"Notița a fost mutată în categoria '{existingCategory.Name}'!";
                }
                else
                {
                    // Creează categoria nouă
                    var newCategory = new Category
                    {
                        Name = NewCategoryName.Trim(),
                        Color = GenerateRandomColor()
                    };
                    _dbContext.Categories.Add(newCategory);
                    await _dbContext.SaveChangesAsync();

                    note.CategoryId = newCategory.Id;
                    Message = $"Categoria '{newCategory.Name}' a fost creată și notița a fost adăugată!";
                }
            }
            else
            {
                // Folosește categoria selectată din dropdown
                note.CategoryId = NewCategoryId;
                var categoryName = NewCategoryId.HasValue
                    ? (await _dbContext.Categories.FindAsync(NewCategoryId.Value))?.Name ?? "Necunoscut"
                    : "Fără categorie";
                Message = $"Notița a fost mutată în '{categoryName}'!";
            }

            note.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            MessageType = "success";
            return RedirectToPage(new { SelectedCategoryId });
        }

        private async Task LoadDataAsync()
        {
            // Încarcă toate categoriile
            Categories = await _dbContext.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            // Încarcă notițele (cu sau fără filtrare)
            var query = _dbContext.Notes.Include(n => n.Category).AsQueryable();

            if (SelectedCategoryId.HasValue)
            {
                // Filtrează după categoria selectată
                query = query.Where(n => n.CategoryId == SelectedCategoryId.Value);
            }

            Notes = await query
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        private string GenerateRandomColor()
        {
            var colors = new[]
            {
                "#6366f1", // indigo
                "#8b5cf6", // violet
                "#ec4899", // pink
                "#f59e0b", // amber
                "#10b981", // emerald
                "#3b82f6", // blue
                "#ef4444", // red
                "#14b8a6", // teal
            };

            return colors[new Random().Next(colors.Length)];
        }
    }
}
