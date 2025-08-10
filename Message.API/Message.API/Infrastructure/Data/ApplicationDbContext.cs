using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Message.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Message.API.Infrastructure.Data
{
    // Cambiamos el tipo base del DbContext para usar Guid para ApplicationUser y IdentityRole
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // PASO CRUCIAL: Llama al método base primero para que EF configure
            // todas las tablas de Identity. Esto evita el error de la clave.
            base.OnModelCreating(builder);

            // AHORA, configura explícitamente la relación de tu modelo.
            builder
                .Entity<Appointment>()
                .HasOne(a => a.Client)
                .WithMany(u => u.Appointments) // Agregamos WithMany para la relación
                .HasForeignKey(a => a.ClientId);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries().Where(e => e.Entity is BaseEntity);

            foreach (var entityEntry in entries)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    var baseEntity = (BaseEntity)entityEntry.Entity;
                    baseEntity.CreatedAt = DateTime.UtcNow;
                    baseEntity.IsEnabled = true;
                }

                if (
                    entityEntry.State == EntityState.Modified
                    || entityEntry.State == EntityState.Added
                )
                {
                    var baseEntity = (BaseEntity)entityEntry.Entity;
                    baseEntity.ModifiedAt = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
