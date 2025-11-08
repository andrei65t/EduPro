using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using EduPro.Data;
using EduPro.Models;

namespace EduPro.Pages
{
	public class TextQuizModel : PageModel
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;
		private readonly ILogger<TextQuizModel> _logger;
		private readonly AppDbContext _dbContext;

		[BindProperty]
		public string? NotesText { get; set; }

		[BindProperty]
		public string Difficulty { get; set; } = "medium";

		[BindProperty]
		public int NumQuestions { get; set; } = 5;

		[BindProperty]
		public int? SelectedNoteId { get; set; }

		public List<QuizQuestion>? Questions { get; set; }
		public string? ErrorMessage { get; set; }
		public int? Score { get; set; }
		public bool QuizSubmitted { get; set; }

		public List<Note> SavedNotes { get; set; } = new();

		public TextQuizModel(
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration,
			ILogger<TextQuizModel> logger,
			AppDbContext dbContext)
		{
			_httpClientFactory = httpClientFactory;
			_configuration = configuration;
			_logger = logger;
			_dbContext = dbContext;
		}

		public async Task OnGetAsync()
		{
			// Load saved notes for selection
			SavedNotes = await _dbContext.Notes
				.OrderByDescending(n => n.CreatedAt)
				.Take(20)
				.ToListAsync();
		}

		public async Task<IActionResult> OnPostGenerateAsync()
		{
			// Load saved notes first
			SavedNotes = await _dbContext.Notes
				.OrderByDescending(n => n.CreatedAt)
				.Take(20)
				.ToListAsync();

			// If a note is selected, use its text
			if (SelectedNoteId.HasValue)
			{
				var selectedNote = await _dbContext.Notes.FindAsync(SelectedNoteId.Value);
				if (selectedNote != null)
				{
					NotesText = selectedNote.ExtractedText;
				}
			}

			if (string.IsNullOrWhiteSpace(NotesText) || NotesText.Length < 50)
			{
				ErrorMessage = "Te rog introdu cel puțin 50 de caractere de text pentru a genera un quiz.";
				return Page();
			}

			try
			{
				var ocrApiUrl = Environment.GetEnvironmentVariable("OCR_API_URL") 
					?? _configuration["OCR_API_URL"] 
					?? "http://localhost:8001";

				_logger.LogInformation("Calling Quiz API at {OcrApiUrl}", ocrApiUrl);

				var client = _httpClientFactory.CreateClient();
				client.Timeout = TimeSpan.FromSeconds(60);

				var requestBody = new
				{
					text = NotesText,
					difficulty = Difficulty,
					num_questions = NumQuestions
				};

				var jsonContent = new StringContent(
					JsonSerializer.Serialize(requestBody),
					Encoding.UTF8,
					"application/json");

				var response = await client.PostAsync($"{ocrApiUrl}/generate-quiz", jsonContent);

				if (response.IsSuccessStatusCode)
				{
					var jsonResponse = await response.Content.ReadAsStringAsync();
					_logger.LogInformation("Quiz API response received");

					var result = JsonSerializer.Deserialize<QuizResponse>(jsonResponse, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					Questions = result?.questions ?? new List<QuizQuestion>();
					
					if (Questions.Count == 0)
					{
						ErrorMessage = "Nu s-au generat întrebări. Te rog încearcă din nou cu un text mai detaliat.";
					}
				}
				else
				{
					var errorContent = await response.Content.ReadAsStringAsync();
					ErrorMessage = $"Eroare Quiz API: {response.StatusCode}";
					_logger.LogError("Quiz API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
				}
			}
			catch (HttpRequestException ex)
			{
				ErrorMessage = "Nu pot contacta serverul de quiz. Verifică dacă serverul rulează.";
				_logger.LogError(ex, "HTTP error calling Quiz API");
			}
			catch (Exception ex)
			{
				ErrorMessage = $"Eroare la generarea quiz-ului: {ex.Message}";
				_logger.LogError(ex, "Error generating quiz");
			}

			return Page();
		}

		public async Task<IActionResult> OnPostSubmitAsync()
		{
			// Reconstruct questions from hidden fields
			var questionsJson = Request.Form["QuestionsJson"].ToString();
			if (!string.IsNullOrEmpty(questionsJson))
			{
				Questions = JsonSerializer.Deserialize<List<QuizQuestion>>(questionsJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});
			}

			if (Questions == null || Questions.Count == 0)
			{
				ErrorMessage = "Nu există întrebări pentru evaluare.";
				return Page();
			}

			// Calculate score - get user answers from form
			int correctAnswers = 0;
			for (int i = 0; i < Questions.Count; i++)
			{
				var answerKey = $"answer_{i}";
				if (Request.Form.ContainsKey(answerKey))
				{
					if (int.TryParse(Request.Form[answerKey], out int userAnswer))
					{
						if (userAnswer == Questions[i].correct_answer)
						{
							correctAnswers++;
						}
					}
				}
			}

			Score = (int)((double)correctAnswers / Questions.Count * 100);
			QuizSubmitted = true;

			// Reload notes for display
			SavedNotes = await _dbContext.Notes
				.OrderByDescending(n => n.CreatedAt)
				.Take(20)
				.ToListAsync();

			return Page();
		}

		public class QuizQuestion
		{
			public string question { get; set; } = string.Empty;
			public List<string> options { get; set; } = new();
			public int correct_answer { get; set; }
			public string explanation { get; set; } = string.Empty;
		}

		public class QuizResponse
		{
			public List<QuizQuestion> questions { get; set; } = new();
		}
	}
}
