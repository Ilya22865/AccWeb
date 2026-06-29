using DriverService.Models;
using Microsoft.EntityFrameworkCore;

namespace DriverService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Driver> Drivers => Set<Driver>();
}
