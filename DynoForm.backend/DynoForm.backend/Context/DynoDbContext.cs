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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Form>()
            .Property(f => f.Id)
            .HasColumnType("uuid");

        modelBuilder.Entity<FormData>()
            .Property(f => f.Id)
            .HasColumnType("uuid");

        modelBuilder.Entity<FormData>()
            .Property(f => f.FormId)
            .HasColumnType("uuid");

        modelBuilder.Entity<FormFieldData>()
            .Property(f => f.Id)
            .HasColumnType("uuid");

        modelBuilder.Entity<FormFieldData>()
            .Property(f => f.FormDataId)
            .HasColumnType("uuid");
    }
}
