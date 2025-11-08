using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduPro.Pages
{
	public class AskNotesModel : PageModel
	{
		[BindProperty]
		public string? Question { get; set; }

		[BindProperty]
		public string? SelectedNotesSet { get; set; }

		// În backend: vei avea aici info dacă a găsit în notițe sau nu, plus răspunsul
		public bool IsFromNotes { get; set; }
		public string? Answer { get; set; }

		public void OnGet()
		{
		}

		public IActionResult OnPost()
		{
			// TODO:
			// 1. Caută prin notițe (DB, fișiere) dacă există conținut relevant.
			// 2. Dacă da -> setezi IsFromNotes = true și Answer = ...
			// 3. Dacă nu -> IsFromNotes = false, Answer = rezumat general.
			return Page();
		}
	}
}
	