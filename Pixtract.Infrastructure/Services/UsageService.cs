using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pixtract.Application.Interfaces;
using Pixtract.Domain.Entities;
using Pixtract.Infrastructure.Data;

namespace Pixtract.Infrastructure.Services;

public class UsageService : IUsageService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UsageService> _logger;

    public UsageService(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
        ILogger<UsageService> logger)
    {
        _db = db;
        _userManager = userManager;
        _logger = logger;
    }


    // verific daca user ul mai are voie sa proceseze imagini astazi
    public async Task<bool> CanProcessAsync(string userId, int imagesToProcess = 1)
    {
        // PAS 1  caut utilizatorul in db
        var user = await _userManager.Users
            .Include(u => u.Plan)
            .FirstOrDefaultAsync(u => u.Id == userId);

        //daca nu exista-STOP procesare
        if (user == null)
        {
            _logger.LogWarning("Utilizatorul nu a fost gasit. Cererea nu poate fi procesata. UserId={UserId}", userId);
            return false;
        }

        var todayUsage = await GetTodayUsageAsync(userId);
        var canProcess = (todayUsage + imagesToProcess) <= user.Plan.DailyImageLimit;

        _logger.LogInformation(
            "Verificare limita utilizare pentru utilizatorul: {UserId}. Plan={Plan}, TodayUsage={Used}, Cerute={Requested}, Limita={Limit}, PoateProcesa={CanProcess}",
            userId, user.Plan.Name, todayUsage, imagesToProcess, user.Plan.DailyImageLimit, canProcess);

        return canProcess;
    }

    //cresc numarul de imagini folosite de utilizator astazi
    public async Task IncrementAsync(string userId, int count = 1)
    {
        var today = DateTime.UtcNow.Date;

        var usage = await _db.UserDailyUsages
            .FirstOrDefaultAsync(u => u.UserId == userId && u.Date == today);

        //daca nu a fot usage, se dauaga direct count, altfel se adauga peste usage ul actual count ul
        if (usage == null)
        {
            usage = new UserDailyUsage
            {
                UserId = userId,
                Date = today,
                ImagesUsed = count
            };
            _db.UserDailyUsages.Add(usage);
            _logger.LogInformation(
                "S-a creat un nou record de utilizare zilnica pentru utilizatorul: {UserId}. Numar initial={Count}",
                userId, count);
        }
        else
        {
            usage.ImagesUsed += count;
            _logger.LogInformation(
                "S-a actualizat utilizarea zilnica pentru utilizatorul: {UserId}. Total imagini azi={Total}",
                userId, usage.ImagesUsed);
        }

        await _db.SaveChangesAsync();
    }


    // returnez cate imagini a folosit utilizatorul astazi
    public async Task<int> GetTodayUsageAsync(string userId)
    {
        var today = DateTime.UtcNow.Date;

        var usage = await _db.UserDailyUsages
            .FirstOrDefaultAsync(u => u.UserId == userId && u.Date == today);

        var count = usage?.ImagesUsed ?? 0;
        _logger.LogDebug("Utilizare de azi pentru utilizatorul: {UserId}. ImagesUsed ={Count}", userId, count);
        return count;
    }


    //generez urmatorul ProductId pentru utilizator
    public async Task<string> GetNextProductIdAsync(string userId)
    {
        var todayUsage = await GetTodayUsageAsync(userId);

        //generez urmatorul ProductId pe baza utilizarii de azi
        //exemplu: 4 imagini folosite inseamna P005
        var productId = $"P{(todayUsage + 1):D3}";
        _logger.LogDebug("ID produs generat pentru utilizatorul {UserId}. ProductId={ProductId}", userId, productId);
        return productId;
    }

    // returnez cate imagini are voie utilizatorul sa trimita intr un singur request
    public async Task<int> GetImagesPerRequestAsync(string userId)
    {
        var user = await _userManager.Users
            .Include(u => u.Plan)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var limit = user?.Plan?.ImagesPerRequest ?? 1;
        _logger.LogDebug("Imagini permise per request pentru utilizatorul {UserId}. Limita={Limit}", userId, limit);
        return limit;
    }
}