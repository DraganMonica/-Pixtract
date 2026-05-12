using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pixtract.Domain.Entities;

namespace Pixtract.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<ExtractionRequest> ExtractionRequests => Set<ExtractionRequest>();
    public DbSet<BatchJob> BatchJobs => Set<BatchJob>();
    public DbSet<UserDailyUsage> UserDailyUsages => Set<UserDailyUsage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}