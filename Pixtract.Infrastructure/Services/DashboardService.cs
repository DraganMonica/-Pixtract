using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pixtract.Application.DTOs;
using Pixtract.Application.Interfaces;
using Pixtract.Domain.Entities;
using Pixtract.Infrastructure.Data;

namespace Pixtract.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _db;
    private readonly IUsageService _usageService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(ApplicationDbContext db, IUsageService usageService,
        UserManager<ApplicationUser> userManager, ILogger<DashboardService> logger)
    {
        _db = db;
        _usageService = usageService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<int> GetTodayUsageAsync(string userId)
        => await _usageService.GetTodayUsageAsync(userId);

    public async Task<int> GetDailyLimitAsync(string userId)
    {
        var user = await _userManager.Users
            .Include(u => u.Plan)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var limit = user?.Plan?.DailyImageLimit ?? 0;
        _logger.LogDebug("Limita zilnica recuperata pentru utilizatorul {UserId}. Limita={Limit}", userId, limit);
        return limit;
    }

    public async Task<string> GetPlanNameAsync(string userId)
    {
        var user = await _userManager.Users
            .Include(u => u.Plan)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var planName = user?.Plan?.Name ?? "Free";
        _logger.LogDebug("Plan curent pentru utilizatorul {UserId}. Plan={PlanName}", userId, planName);
        return planName;
    }

    public async Task<List<ExtractionDto>> GetRecentExtractionsAsync(string userId, int count = 10)
    {
        var items = await _db.ExtractionRequests
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .Take(count)
            .ToListAsync();

        _logger.LogInformation("Extractii recente recuperate pentru utilizatorul {UserId}. Total={Count}", userId, items.Count);

        return items.Select(e => new ExtractionDto
        {
            Id = e.Id,
            ProductId = e.ProductId,
            Category = e.Category,
            Status = e.Status.ToString(),
            CreatedAt = e.CreatedAt,
            ImagePath = e.ImagePath,
            Attributes = string.IsNullOrEmpty(e.ResultJson)
                ? new()
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(e.ResultJson) ?? new()
        }).ToList();
    }
}