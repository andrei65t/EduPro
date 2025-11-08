using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduPro.Pages
{
	public class TextQuizModel : PageModel
	{
		[BindProperty]
		public string? NotesText { get; set; }

		[BindProperty]
		public string Difficulty { get; set; } = "medium";

		// Aici vei ține întrebările generate, scorul etc.
		public void OnGet()
		{
		}

		public IActionResult OnPost()
		{
			// TODO: apelează un serviciu care generează quiz-ul din NotesText
			// Deocamdată nu facem nimic.
			return Page();
		}
	}
}
