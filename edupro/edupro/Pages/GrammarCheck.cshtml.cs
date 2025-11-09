using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EduPro.Data;
using EduPro.Models;
using System.Text.Json;
using System.Text;

namespace EduPro.Pages
{
	public class GrammarCheckModel : PageModel
	{
		private readonly AppDbContext _dbContext;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;
		private readonly ILogger<GrammarCheckModel> _logger;

		[BindProperty]
		public string? InputText { get; set; }

		[BindProperty]
		public int? SelectedNoteId { get; set; }

		[BindProperty]
		public string InputSource { get; set; } = "manual";

		public List<Note> AvailableNotes { get; set; } = new();

		// Aici vei stoca textul corectat și corecțiile
		public string? CorrectedText { get; set; }
		public List<string> Corrections { get; set; } = new();

		public GrammarCheckModel(AppDbContext dbContext, IHttpClientFactory httpClientFactory,
			IConfiguration configuration, ILogger<GrammarCheckModel> logger)
		{
			_dbContext = dbContext;
			_httpClientFactory = httpClientFactory;
			_configuration = configuration;
			_logger = logger;
		}

		public async Task OnGetAsync()
		{
			await LoadNotesAsync();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			await LoadNotesAsync();

			string textToCheck = string.Empty;

			// Determină sursa textului
			if (InputSource == "note" && SelectedNoteId.HasValue)
			{
				var selectedNote = AvailableNotes.FirstOrDefault(n => n.Id == SelectedNoteId.Value);
				if (selectedNote != null)
				{
					textToCheck = selectedNote.ExtractedText;
				}
			}
			else if (!string.IsNullOrWhiteSpace(InputText))
			{
				textToCheck = InputText;
			}

			if (string.IsNullOrWhiteSpace(textToCheck))
			{
				return Page();
			}

			try
			{
				// Apelează API-ul pentru corectarea gramaticală
				var apiUrl = _configuration["OcrApiUrl"] ?? "http://localhost:8001";
				_logger.LogInformation($"Calling Grammar Check API at {apiUrl}");

				var httpClient = _httpClientFactory.CreateClient();
				var requestBody = new { text = textToCheck };

				var jsonContent = new StringContent(
					JsonSerializer.Serialize(requestBody),
					Encoding.UTF8,
					"application/json"
				);

				var response = await httpClient.PostAsync($"{apiUrl}/grammar-check", jsonContent);

				if (response.IsSuccessStatusCode)
				{
					var responseContent = await response.Content.ReadAsStringAsync();
					var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

					CorrectedText = result.GetProperty("corrected_text").GetString();

					if (result.TryGetProperty("corrections", out JsonElement correctionsElement))
					{
						Corrections = correctionsElement.EnumerateArray()
							.Select(c => c.GetString() ?? string.Empty)
							.Where(c => !string.IsNullOrEmpty(c))
							.ToList();
					}
				}
				else
				{
					_logger.LogError($"Grammar Check API error: {response.StatusCode}");
					CorrectedText = "Eroare la verificarea gramaticală. Te rog încearcă din nou.";
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Exception in Grammar Check: {ex.Message}");
				CorrectedText = "A apărut o eroare la verificarea gramaticală.";
			}

			return Page();
		}

		private async Task LoadNotesAsync()
		{
			AvailableNotes = await _dbContext.Notes
				.Include(n => n.Category)
				.OrderByDescending(n => n.CreatedAt)
				.ToListAsync();
		}
	}
}