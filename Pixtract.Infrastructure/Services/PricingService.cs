using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pixtract.Application.Interfaces;
using Pixtract.Domain.Entities;
using Pixtract.Infrastructure.Data;

namespace Pixtract.Infrastructure.Services;

public class PricingService : IPricingService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<PricingService> _logger;

    public PricingService(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
        ILogger<PricingService> logger)
    {
        _db = db;
        _userManager = userManager;
        _logger = logger;
    }

    //returnez toate planurile
    public async Task<List<Plan>> GetAllPlansAsync()
    {
        var plans = await _db.Plans.OrderBy(p => p.Price).ToListAsync();
        _logger.LogDebug("Planuri disponibile recuperate din baza de date. Total={Count}", plans.Count);
        return plans;
    }

    // returnez un plan specific dupa Id
    public async Task<Plan?> GetPlanByIdAsync(int planId)
    {
        var plan = await _db.Plans.FirstOrDefaultAsync(p => p.Id == planId);
        if (plan == null)
            _logger.LogWarning("Planul cu Id={PlanId} nu a fost gasit in baza de date.", planId);
        else
            _logger.LogDebug("Plan gasit. Id={PlanId}, Nume={PlanName}", planId, plan.Name);
        return plan;
    }

    // returnez planul asociat utilizatorului curent
    public async Task<Plan?> GetUserPlanAsync(string userId)
    {
        var user = await _userManager.Users
            .Include(u => u.Plan)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var plan = user?.Plan;
        if (plan == null)
            _logger.LogWarning("Utilizatorul {UserId} nu are niciun plan asociat.", userId);
        else
            _logger.LogDebug("Plan curent pentru utilizatorul {UserId}. Plan={PlanName}", userId, plan.Name);

        return plan;
    }
}