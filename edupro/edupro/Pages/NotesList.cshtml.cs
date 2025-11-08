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

		public NotesListModel(AppDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task OnGetAsync()
		{
			// Încarcă toate notițele, sortate de la cele mai recente
			Notes = await _dbContext.Notes
				.OrderByDescending(n => n.CreatedAt)
				.ToListAsync();
		}
	}
}
