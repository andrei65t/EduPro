using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

public class Courses_DetailsModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Slug { get; set; }

    public string CourseTitle { get; set; }

    public List<string> Lessons { get; set; }

    public List<string> Notes { get; set; }

    public void OnGet()
    {
        // Map slug to a human-friendly title and sample content.
        switch((Slug ?? "").ToLowerInvariant())
        {
            case "matematica":
                CourseTitle = "Matematică";
                Lessons = new List<string> { "Algebră - Capitolul 1", "Geometrie - Capitolul 2", "Funcții - Capitolul 3" };
                Notes = new List<string> { "Notițe: Algebră - ecuații", "Notițe: Geometrie - triunghiuri" };
                break;
            case "romana":
                CourseTitle = "Română";
                Lessons = new List<string> { "Literatură - Lectură", "Gramatică - Părți de vorbire" };
                Notes = new List<string> { "Notițe: Analiză text", "Notițe: Conjugări" };
                break;
            case "chimie":
                CourseTitle = "Chimie";
                Lessons = new List<string> { "Structura atomului", "Reacții chimice" };
                Notes = new List<string> { "Notițe: Tabele reactivitate" };
                break;
            case "fizica":
                CourseTitle = "Fizică";
                Lessons = new List<string> { "Mecanica", "Termodinamica" };
                Notes = new List<string> { };
                break;
            case "biologie":
                CourseTitle = "Biologie";
                Lessons = new List<string> { "Celula", "Genetica" };
                Notes = new List<string> { "Notițe: ADN și ARN" };
                break;
            case "engleza":
                CourseTitle = "Engleză";
                Lessons = new List<string> { "Grammar - basics", "Vocabulary" };
                Notes = new List<string> { };
                break;
            default:
                CourseTitle = "Curs necunoscut";
                Lessons = new List<string>();
                Notes = new List<string>();
                break;
        }
    }
}
