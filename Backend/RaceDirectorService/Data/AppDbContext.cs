using Microsoft.EntityFrameworkCore;

namespace RaceDirectorService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
}
