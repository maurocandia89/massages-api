using Message.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Message.API.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Appointment> Appointments { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries().Where(e => e.Entity is BaseEntity);

        foreach (var entityEntry in entries)
        {
            // Si la entidad se está creando
            if (entityEntry.State == EntityState.Added)
            {
                var baseEntity = (BaseEntity)entityEntry.Entity;
                baseEntity.CreatedAt = DateTime.UtcNow;
                baseEntity.IsEnabled = true;
            }

            // Si la entidad se está modificando o creando, actualiza ModifiedAt
            if (entityEntry.State == EntityState.Modified || entityEntry.State == EntityState.Added)
            {
                var baseEntity = (BaseEntity)entityEntry.Entity;
                baseEntity.ModifiedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
