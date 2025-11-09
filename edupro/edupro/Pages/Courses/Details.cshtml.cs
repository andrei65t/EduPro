using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EduPro.Data;
using EduPro.Models;

public class Courses_DetailsModel : PageModel
{
    private readonly AppDbContext _db;

    public Courses_DetailsModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public string? Slug { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Lesson { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Date { get; set; }

    public string? CourseTitle { get; set; }

    // legacy property kept for compatibility with the view
    public List<string> Lessons { get; set; } = new List<string>();

    // legacy Notes list (strings) kept but we'll populate NotesData for real notes
    public List<string> Notes { get; set; } = new List<string>();

    // Real notes loaded from DB
    public List<Note> NotesData { get; set; } = new List<Note>();

    public void OnGet()
    {
        var normalized = (Slug ?? string.Empty).Trim().ToLowerInvariant();

        // try to find category by exact name or by slugified name
        var category = _db.Categories
            .FirstOrDefault(c => c.Name.ToLower() == normalized || c.Name.ToLower().Replace(" ", "-") == normalized);

        if (category == null)
        {
            CourseTitle = "Curs necunoscut";
            return;
        }

        CourseTitle = category.Name;

        // Load notes for this category, newest first
        var notesQuery = _db.Notes
            .Where(n => n.CategoryId == category.Id)
            .OrderByDescending(n => n.CreatedAt);

        var notes = notesQuery.ToList();

        // Apply optional filters from query string
        if (!string.IsNullOrWhiteSpace(Lesson))
        {
            var decodedLesson = System.Net.WebUtility.UrlDecode(Lesson);
            notes = notes.Where(n => (n.Title ?? "Fără denumire") == decodedLesson).ToList();
            CourseTitle = category.Name + " — " + decodedLesson;
        }

        if (!string.IsNullOrWhiteSpace(Date))
        {
            if (DateTime.TryParse(Date, out var parsedDate))
            {
                notes = notes.Where(n => n.CreatedAt.Date == parsedDate.Date).ToList();
                CourseTitle = category.Name + " — " + parsedDate.ToString("dd MMMM yyyy");
            }
        }

        NotesData = notes;

        // Keep simple string list for any old code expecting Model.Notes
        Notes = NotesData.Select(n => (n.Title ?? n.OriginalFileName) ?? n.ExtractedText?.Substring(0, Math.Min(80, n.ExtractedText.Length)) ?? "Notiță").ToList();
    }
}
