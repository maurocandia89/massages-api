using Message.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Message.API.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Appointment> Appointments { get; set; }
}
