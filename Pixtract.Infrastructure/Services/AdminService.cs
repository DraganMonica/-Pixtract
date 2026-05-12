using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pixtract.Application.DTOs;
using Pixtract.Application.Interfaces;
using Pixtract.Domain.Entities;
using Pixtract.Infrastructure.Data;

namespace Pixtract.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminService> _logger;

    public AdminService(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
        ILogger<AdminService> logger)
    {
        _db = db;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _userManager.Users
            .Include(u => u.Plan)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserDto
            {
                Email = u.Email,
                PlanName = u.Plan != null ? u.Plan.Name : "Free",
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        _logger.LogInformation("Lista utilizatori recuperata de admin. Total utilizatori={Count}", users.Count);
        return users;
    }

    public async Task<int> GetTotalExtractionsAsync()
    {
        var total = await _db.ExtractionRequests.CountAsync();
        _logger.LogInformation("Total extrageri in sistem={Total}", total);
        return total;
    }

    public async Task<int> GetTodayExtractionsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var count = await _db.ExtractionRequests
            .CountAsync(e => e.CreatedAt.Date == today);

        _logger.LogInformation("Extrageri efectuate azi={Count}, Data={Date}", count, today.ToString("dd/MM/yyyy"));
        return count;
    }

    public async Task<int> GetActiveUsersTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _db.UserDailyUsages
            .Where(u => u.Date == today && u.ImagesUsed > 0)
            .Select(u => u.UserId)
            .Distinct()
            .CountAsync();
    }

    public async Task<int> GetExpiringSoonAsync()
    {
        var now = DateTime.UtcNow;
        var in7Days = now.AddDays(7);
        return await _userManager.Users
            .CountAsync(u => u.PlanExpiresAt.HasValue &&
                             u.PlanExpiresAt.Value >= now &&
                             u.PlanExpiresAt.Value <= in7Days);
    }

    public async Task<List<CategoryStatDto>> GetTopCategoriesAsync()
    {
        return await _db.ExtractionRequests
            .GroupBy(e => e.Category)
            .Select(g => new CategoryStatDto { Category = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();
    }
}