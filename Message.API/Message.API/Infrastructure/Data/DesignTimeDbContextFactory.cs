using Message.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Message.API.Infrastructure.Data
{
    // Esta clase le dice a las herramientas de Entity Framework Core
    // cómo crear una instancia de ApplicationDbContext durante el diseño (ej. migraciones).
    // Esto es necesario porque las herramientas de EF Core no inician la aplicación
    // de la misma manera que lo hace el runtime.
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Construye una configuración simple para obtener la cadena de conexión
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Si la cadena de conexión es null, lanza un error para que sepamos qué pasa.
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "Could not find connection string 'DefaultConnection'."
                );
            }

            builder.UseNpgsql(connectionString);

            return new ApplicationDbContext(builder.Options);
        }
    }
}
