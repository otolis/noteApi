using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NotesApi.Data;

public class AppDbFactory : IDesignTimeDbContextFactory<AppDb>
{
    public AppDb CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<AppDb>();
        builder.UseSqlite("Data Source=notes.db");
        return new AppDb(builder.Options);
    }
}
