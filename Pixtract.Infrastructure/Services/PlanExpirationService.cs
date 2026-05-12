using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pixtract.Infrastructure.Data;

namespace Pixtract.Infrastructure.Services;

public class PlanExpirationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PlanExpirationService> _logger;

    public PlanExpirationService(IServiceScopeFactory scopeFactory, ILogger<PlanExpirationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await DowngradeExpiredUsersAsync();
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task DowngradeExpiredUsersAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var freePlan = await db.Plans.FirstOrDefaultAsync(p => p.Name == "Free");
        if (freePlan == null)
        {
            _logger.LogWarning("Planul Free nu a fost gasit in baza de date. Downgrade-ul nu poate fi efectuat.");
            return;
        }

        var expiredUsers = await db.Users
            .Where(u => u.PlanExpiresAt != null
                     && u.PlanExpiresAt < DateTime.UtcNow
                     && u.PlanId != freePlan.Id)
            .ToListAsync();

        if (!expiredUsers.Any())
        {
            _logger.LogInformation("Niciun utilizator cu plan expirat gasit.");
            return;
        }

        foreach (var user in expiredUsers)
        {
            _logger.LogInformation("Plan expirat pentru utilizatorul {UserId}. Downgrade la Free.", user.Id);
            user.PlanId = freePlan.Id;
            user.PlanExpiresAt = null;
        }

        await db.SaveChangesAsync();
        _logger.LogInformation("Downgrade efectuat pentru {Count} utilizatori cu plan expirat.", expiredUsers.Count);
    }
}
