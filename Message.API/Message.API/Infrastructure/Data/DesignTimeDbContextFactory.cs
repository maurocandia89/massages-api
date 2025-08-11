using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Message.API.Infrastructure.Data;

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

        // Aquí es donde se especifica que usas PostgreSQL
        builder.UseNpgsql(connectionString);

        // Se usa el constructor de diseño para crear el DbContext.
        return new ApplicationDbContext(builder.Options);
    }
}
