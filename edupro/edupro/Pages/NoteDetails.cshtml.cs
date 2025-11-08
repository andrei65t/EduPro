using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EduPro.Data;
using EduPro.Models;

namespace EduPro.Pages
{
	public class NoteDetailsModel : PageModel
	{
		private readonly AppDbContext _dbContext;

		public Note? Note { get; set; }

		public NoteDetailsModel(AppDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<IActionResult> OnGetAsync(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			Note = await _dbContext.Notes.FindAsync(id);

			if (Note == null)
			{
				return NotFound();
			}

			return Page();
		}

		public async Task<IActionResult> OnPostDeleteAsync(int id)
		{
			var note = await _dbContext.Notes.FindAsync(id);

			if (note != null)
			{
				_dbContext.Notes.Remove(note);
				await _dbContext.SaveChangesAsync();
			}

			return RedirectToPage("/NotesList");
		}
	}
}
