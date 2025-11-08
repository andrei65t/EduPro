using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduPro.Pages
{
	public class NotesUploadModel : PageModel
	{
		[BindProperty]
		public IFormFile? NotesFile { get; set; }

		[BindProperty]
		public string? Title { get; set; }

		// Aici vei stoca textul extras (când faci backend)
		public string? ExtractedText { get; set; }

		public void OnGet()
		{
		}

		public async Task<IActionResult> OnPostAsync()
		{
			// TODO: implementează OCR + salvarea textului într-o bază de date sau fișiere
			// Deocamdată doar rămânem pe pagină, fără logică.
			if (NotesFile != null)
			{
				// aici vei citi stream-ul fișierului
			}

			return Page();
		}
	}
}
