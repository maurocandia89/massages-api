using System.Security.Claims;
using Message.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Message.API.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        _httpContextAccessor = null;
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IHttpContextAccessor httpContextAccessor
    )
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Treatment> Treatments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder
            .Entity<Appointment>()
            .HasOne(a => a.Treatment)
            .WithMany()
            .HasForeignKey(a => a.TreatmentId)
            .IsRequired();
    }

    public override int SaveChanges()
    {
        AddTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void AddTimestamps()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e =>
                e.Entity is BaseEntity
                && (e.State == EntityState.Added || e.State == EntityState.Modified)
            );

        var userIdClaim = _httpContextAccessor?.HttpContext?.User.FindFirst(
            ClaimTypes.NameIdentifier
        );
        var currentUserId = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;

        foreach (var entityEntry in entries)
        {
            var baseEntity = (BaseEntity)entityEntry.Entity;

            if (entityEntry.State == EntityState.Added)
            {
                baseEntity.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                baseEntity.ModifiedAt = DateTime.UtcNow;
            }
        }
    }
}
