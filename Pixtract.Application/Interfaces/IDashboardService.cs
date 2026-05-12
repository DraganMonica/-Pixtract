using Pixtract.Application.DTOs;

namespace Pixtract.Application.Interfaces;

public interface IDashboardService
{
    Task<int> GetTodayUsageAsync(string userId);
    Task<int> GetDailyLimitAsync(string userId);
    Task<string> GetPlanNameAsync(string userId);
    Task<List<ExtractionDto>> GetRecentExtractionsAsync(string userId, int count = 10);
}
