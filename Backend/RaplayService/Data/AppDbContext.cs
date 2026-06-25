using Microsoft.EntityFrameworkCore;

namespace RaplayService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
}
