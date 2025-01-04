using DynoForm.backend.Models;
using Microsoft.EntityFrameworkCore;

namespace DynoForm.backend.Context;

public class DynoDbContext : DbContext
{
    public DbSet<Form> Forms { get; set; }
    public DbSet<FormData> FormData { get; set; }
    public DbSet<FormFieldData> FormFieldData { get; set; }

    public DynoDbContext(DbContextOptions<DynoDbContext> options) : base(options)
    {
        
    }
}
