using Microsoft.EntityFrameworkCore;
using EduPro.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlite("Data Source=edupro.db"));

var app = builder.Build();

// Creează baza de date automat la fiecare pornire
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		var context = services.GetRequiredService<AppDbContext>();

		// Șterge și recrează baza de date
		context.Database.EnsureDeleted();
		context.Database.EnsureCreated();

		Console.WriteLine("✅ Baza de date a fost creată cu succes!");
	}
	catch (Exception ex)
	{
		Console.WriteLine($"❌ Eroare la crearea bazei de date: {ex.Message}");
	}
}

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();