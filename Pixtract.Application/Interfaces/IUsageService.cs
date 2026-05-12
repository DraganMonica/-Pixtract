namespace Pixtract.Application.Interfaces;

public interface IUsageService
{
    Task<bool> CanProcessAsync(string userId, int imagesToProcess = 1);
    Task IncrementAsync(string userId, int count = 1);
    Task<int> GetTodayUsageAsync(string userId);
    Task<string> GetNextProductIdAsync(string userId);
    Task<int> GetImagesPerRequestAsync(string userId);
}
