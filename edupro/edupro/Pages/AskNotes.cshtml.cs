using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EduPro.Data;
using EduPro.Models;

namespace EduPro.Pages
{
	public class AskNotesModel : PageModel
	{
		private readonly AppDbContext _dbContext;

		[BindProperty]
		public string? Question { get; set; }

		[BindProperty(SupportsGet = true)]
		public int? SelectedCategoryId { get; set; }

		public List<Note> FilteredNotes { get; set; } = new();
		public List<Category> Categories { get; set; } = new();

		public bool HasAskedQuestion { get; set; }
		public bool IsFromNotes { get; set; }
		public string? Answer { get; set; }
		public List<string> SourceNotes { get; set; } = new();

		public string CurrentFilter => SelectedCategoryId.HasValue
			? Categories.FirstOrDefault(c => c.Id == SelectedCategoryId.Value)?.Name ?? "Necunoscut"
			: "Toate categoriile";

		public AskNotesModel(AppDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task OnGetAsync()
		{
			await LoadDataAsync();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			await LoadDataAsync();

			if (string.IsNullOrWhiteSpace(Question))
			{
				return Page();
			}

			HasAskedQuestion = true;

			if (FilteredNotes.Any())
			{
				IsFromNotes = true;
				var relevantNotes = FilteredNotes.Take(3).ToList();
				SourceNotes = relevantNotes.Select(n => n.Title ?? "Fără titlu").ToList();

				var combinedText = string.Join("\n\n", relevantNotes.Select(n =>
					$"Din '{n.Title}':\n{n.ExtractedText.Substring(0, Math.Min(300, n.ExtractedText.Length))}..."
				));

				Answer = $"Am găsit informații în {relevantNotes.Count} {(relevantNotes.Count == 1 ? "notiță" : "notițe")} " +
						 $"din categoria '{CurrentFilter}':\n\n{combinedText}\n\n" +
						 $"[Aici vei integra răspunsul generat de AI]";
			}
			else
			{
				IsFromNotes = false;
				Answer = $"Nu există notițe în categoria '{CurrentFilter}'.";
			}

			return Page();
		}

		private async Task LoadDataAsync()
		{
			Categories = await _dbContext.Categories.OrderBy(c => c.Name).ToListAsync();

			var query = _dbContext.Notes.Include(n => n.Category).AsQueryable();

			if (SelectedCategoryId.HasValue)
			{
				query = query.Where(n => n.CategoryId == SelectedCategoryId.Value);
			}

			FilteredNotes = await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
		}
	}
}