using EvilGiraf.Model;
using Microsoft.EntityFrameworkCore;

namespace EvilGiraf.Service;

public class DatabaseService : DbContext
{
    public DbSet<Application> Applications { get; set; }

    public DatabaseService(DbContextOptions<DatabaseService> options) : base(options)
    { }
}