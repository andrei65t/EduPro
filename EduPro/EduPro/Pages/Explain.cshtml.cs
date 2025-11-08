using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduPro.Pages
{
	public class ExplainModel : PageModel
	{
		[BindProperty]
		public string? InputText { get; set; }

		[BindProperty]
		public string Level { get; set; } = "medium";

		// Aici vei stoca explicația generată
		public string? GeneratedExplanation { get; set; }

		public void OnGet()
		{
		}

		public IActionResult OnPost()
		{
			// TODO: apelează un serviciu AI / logică care generează explicația din InputText + Level
			return Page();
		}
	}
}
