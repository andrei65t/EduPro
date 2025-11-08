using Microsoft.EntityFrameworkCore;
using EduPro.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddHttpClient(); // Required for calling OCR API

// Add Entity Framework DbContext (In-Memory pentru dezvoltare)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("EduProDb"));

var app = builder.Build();

// Configure the HTTP request pipeline
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
