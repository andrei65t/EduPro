using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

public class AccountModel : PageModel
{
    private readonly IWebHostEnvironment _env;

    public AccountModel(IWebHostEnvironment env)
    {
        _env = env;
    }

    [BindProperty]
    public string Name { get; set; }

    [BindProperty]
    public string Email { get; set; }

    [BindProperty]
    public string Role { get; set; }

    [BindProperty]
    public IFormFile AvatarUpload { get; set; }

    public string AvatarPath { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    public void OnGet()
    {
        // For demo we use static values; in real app load from DB / auth
        Name = "Andrei Popescu";
        Email = "andrei.popescu@example.com";
        Role = "Student";
        AvatarPath = Url.Content("~/img/avatars/default-avatar.svg");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Simple validation
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email))
        {
            ModelState.AddModelError(string.Empty, "Name and Email are required.");
            AvatarPath = Url.Content("~/img/avatars/default-avatar.svg");
            return Page();
        }

        // Save avatar if uploaded
        if (AvatarUpload != null && AvatarUpload.Length > 0)
        {
            var uploads = Path.Combine(_env.WebRootPath, "img", "avatars");
            var ext = Path.GetExtension(AvatarUpload.FileName);
            var fileName = Guid.NewGuid().ToString("N") + ext;
            var filePath = Path.Combine(uploads, fileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await AvatarUpload.CopyToAsync(fs);
            }

            AvatarPath = Url.Content($"~/img/avatars/{fileName}");
        }
        else
        {
            AvatarPath = Url.Content("~/img/avatars/default-avatar.svg");
        }

        // Simulate save
        StatusMessage = "Profile saved successfully.";

        // Keep values for redisplay
        Role = Role ?? "Student";

        return Page();
    }
}
