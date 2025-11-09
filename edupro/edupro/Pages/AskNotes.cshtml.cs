using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EduPro.Data;
using EduPro.Models;
using System.Text.Json;
using System.Text;

namespace EduPro.Pages
{
	public class AskNotesModel : PageModel
	{
		private readonly AppDbContext _dbContext;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;
		private readonly ILogger<AskNotesModel> _logger;

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
		public string? ErrorMessage { get; set; }

		public string CurrentFilter => SelectedCategoryId.HasValue
			? Categories.FirstOrDefault(c => c.Id == SelectedCategoryId.Value)?.Name ?? "Necunoscut"
			: "Toate categoriile";

		public AskNotesModel(AppDbContext dbContext, IHttpClientFactory httpClientFactory, 
			IConfiguration configuration, ILogger<AskNotesModel> logger)
		{
			_dbContext = dbContext;
			_httpClientFactory = httpClientFactory;
			_configuration = configuration;
			_logger = logger;
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

			if (!FilteredNotes.Any())
			{
				IsFromNotes = false;
				Answer = $"Nu există notițe în categoria '{CurrentFilter}'. Te rog să adaugi notițe sau să schimbi categoria.";
				return Page();
			}

			try
			{
				IsFromNotes = true;
				var relevantNotes = FilteredNotes.Take(5).ToList();
				SourceNotes = relevantNotes.Select(n => n.Title ?? "Fără titlu").ToList();

				// Combinăm textul din notițe pentru context
				var combinedText = string.Join("\n\n", relevantNotes.Select(n =>
					$"=== Din '{n.Title}' ===\n{n.ExtractedText}"
				));

				// Apelăm API-ul pentru Q&A
				var apiUrl = _configuration["OcrApiUrl"] ?? "http://localhost:8001";
				_logger.LogInformation($"Calling Q&A API at {apiUrl}");

				var httpClient = _httpClientFactory.CreateClient();
				var requestBody = new
				{
					question = Question,
					context = combinedText
				};

				var jsonContent = new StringContent(
					JsonSerializer.Serialize(requestBody),
					Encoding.UTF8,
					"application/json"
				);

				var response = await httpClient.PostAsync($"{apiUrl}/ask-question", jsonContent);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					_logger.LogInformation($"Q&A API response received");

					var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
					Answer = result.GetProperty("answer").GetString() ?? "Nu am putut genera un răspuns.";
				}
				else
				{
					var errorContent = await response.Content.ReadAsStringAsync();
					_logger.LogError($"Q&A API error: {response.StatusCode} - {errorContent}");
					ErrorMessage = $"Eroare la apelarea API-ului: {response.StatusCode}";
					Answer = "Nu am putut obține un răspuns din cauza unei erori tehnice.";
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception in Q&A: {ex.Message}");
				ErrorMessage = $"Eroare: {ex.Message}";
				Answer = "A apărut o eroare la procesarea întrebării tale.";
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