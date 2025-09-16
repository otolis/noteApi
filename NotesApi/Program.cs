using Microsoft.EntityFrameworkCore;
using NotesApi.Data;
using NotesApi.Models;

var builder = WebApplication.CreateBuilder(args);

//sql lite sindesi me vasi dedomenon
builder.Services.AddDbContext<AppDb>(opt =>
    opt.UseSqlite("Data Source=notes.db"));

var app = WebApplication.Create();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    db.Database.Migrate();
}

app.MapGet("/notes", async (AppDb db, string? q, int page=1, int pageSize=10)=>
{
    var query = db.Notes.AsQueryable();

    if (!string.IsNullOrWhiteSpace(q))
    {
        query = query.Where(n => n.Title.Contains(q) || n.Content.Contains(q));
    }

    var totalItems = await query.CountAsync();
    var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

    var total = await query.CountAsync();
    var items = await query
        .orderbyDescending(n => n.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return Results.Ok(new { total, page, pageSize, items });
});