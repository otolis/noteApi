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
//lista me anazitisi kai selidopoihsh
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

//get by id
app.MapGet("/notes/{id}", async (AppDb db, int id) =>
{
    var note = await db.Notes.FindAsync(id);
    return note != null ? Results.Ok(note) : Results.NotFound();
});

//dimiourgia neas simiosis
app.MapPost("/notes", async (AppDb db, Note dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title))
       return Results.BadRequest(new { message = "Title is required" });
    var note = new Note { Title = dto.Title.Trim(), Content = dto.Content};
    db.Notes.Add(note);
    await db.SaveChangesAsync();
    return Results.Created($"/notes/{note.Id}", note); 
});

//enhmerwsh simiosis
app.MapPut("/notes/{id:int}", async (AppDb db, int id, Note dto) =>
{
    var note = await db.Notes.FindAsync(id);
    if (note is null) return Results.NotFound();

    if (string.IsNullOrWhiteSpace(dto.Title))
        return Results.BadRequest(new { error = "Title is required." });

    note.Title = dto.Title.Trim();
    note.Content = dto.Content;
    note.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();
    return Results.Ok(note);
});
//diagrafi simiosis
app.MapDelete("/notes/{id:int}", async (AppDb db, int id) =>
{
    var note = await db.Notes.FindAsync(id);
    if (note is null) return Results.NotFound();
    db.Notes.Remove(note);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();