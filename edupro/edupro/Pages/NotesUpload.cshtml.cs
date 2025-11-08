using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using EduPro.Data;
using EduPro.Models;

namespace EduPro.Pages
{
	public class NotesUploadModel : PageModel
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;
		private readonly ILogger<NotesUploadModel> _logger;
		private readonly AppDbContext _dbContext;

		[BindProperty]
		public IFormFile? NotesFile { get; set; }

		[BindProperty]
		public string? Title { get; set; }

		public string? ExtractedText { get; set; }
		public string? Summary { get; set; }
		public string? ErrorMessage { get; set; }
		public bool SavedSuccessfully { get; set; }

		public NotesUploadModel(
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration,
			ILogger<NotesUploadModel> logger,
			AppDbContext dbContext)
		{
			_httpClientFactory = httpClientFactory;
			_configuration = configuration;
			_logger = logger;
			_dbContext = dbContext;
		}

		public void OnGet()
		{
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (NotesFile == null || NotesFile.Length == 0)
			{
				ErrorMessage = "Te rog selectează un fișier valid.";
				return Page();
			}

			try
			{
				var ocrApiUrl = Environment.GetEnvironmentVariable("OCR_API_URL") 
					?? _configuration["OCR_API_URL"] 
					?? "http://localhost:8001";

				_logger.LogInformation("Calling OCR API at {OcrApiUrl}", ocrApiUrl);

				var client = _httpClientFactory.CreateClient();
				client.Timeout = TimeSpan.FromSeconds(30);

				using var content = new MultipartFormDataContent();
				using var fileStream = NotesFile.OpenReadStream();
				using var streamContent = new StreamContent(fileStream);
				
				streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
					NotesFile.ContentType ?? "application/octet-stream");
				content.Add(streamContent, "file", NotesFile.FileName);

				var response = await client.PostAsync($"{ocrApiUrl}/ocr", content);

				if (response.IsSuccessStatusCode)
				{
					var jsonResponse = await response.Content.ReadAsStringAsync();
					_logger.LogInformation("OCR API response: {Response}", jsonResponse);

					var result = JsonSerializer.Deserialize<OcrResponse>(jsonResponse, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					ExtractedText = result?.text_extras ?? "Nu s-a extras niciun text din imagine.";
					Summary = result?.summary ?? "";
					
					if (string.IsNullOrWhiteSpace(ExtractedText) || ExtractedText == "Nu s-a extras niciun text din imagine.")
					{
						ErrorMessage = "Nu am putut extrage text din imagine. Verifică dacă imaginea conține text vizibil.";
					}
					else
					{
						var note = new Note
						{
							ExtractedText = ExtractedText,
							Summary = Summary,
							Title = string.IsNullOrWhiteSpace(Title) ? $"Notiță din {NotesFile.FileName}" : Title,
							OriginalFileName = NotesFile.FileName,
							CreatedAt = DateTime.UtcNow
						};

						_dbContext.Notes.Add(note);
						await _dbContext.SaveChangesAsync();
						
						SavedSuccessfully = true;
						_logger.LogInformation("Note saved with ID: {NoteId}", note.Id);
					}
				}
				else
				{
					var errorContent = await response.Content.ReadAsStringAsync();
					ErrorMessage = $"Eroare OCR API: {response.StatusCode}";
					_logger.LogError("OCR API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
				}
			}
			catch (HttpRequestException ex)
			{
				ErrorMessage = $"Nu pot contacta serverul OCR. Verifică dacă serverul rulează pe portul 8001.";
				_logger.LogError(ex, "HTTP error calling OCR API");
			}
			catch (Exception ex)
			{
				ErrorMessage = $"Eroare la procesarea imaginii: {ex.Message}";
				_logger.LogError(ex, "Error processing image");
			}

			return Page();
		}

		private class OcrResponse
		{
			public string text_extras { get; set; } = string.Empty;
			public string summary { get; set; } = string.Empty;
		}
	}
}
