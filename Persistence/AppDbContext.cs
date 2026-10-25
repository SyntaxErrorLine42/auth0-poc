using _1lab.Entities;
using Microsoft.EntityFrameworkCore;

namespace _1lab.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<Pass> Passes => Set<Pass>();
}
