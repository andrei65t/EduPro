using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EduPro.Data;
using EduPro.Models;
using System.Collections.Generic;
using System.Linq;

namespace EduPro.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly AppDbContext _db;

        public List<Category> Categories { get; set; } = new List<Category>();

        public IndexModel(ILogger<IndexModel> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public void OnGet()
        {
            // Load categories from DB to display on the Index page. Keep this synchronous to avoid changing call sites.
            Categories = _db.Categories
                .OrderBy(c => c.Name)
                .ToList();
        }
    }
}
