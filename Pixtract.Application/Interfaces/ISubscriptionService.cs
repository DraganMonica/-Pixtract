namespace Pixtract.Application.Interfaces;

public interface ISubscriptionService
{
    Task<(bool Success, string Error)> UpgradePlanAsync(string userId, int planId);
    Task<string> CreateFakeCheckoutAsync(string userId, int planId);
}