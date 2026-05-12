using Pixtract.Domain.Entities;

namespace Pixtract.Application.Interfaces;

public interface IPricingService
{
    Task<List<Plan>> GetAllPlansAsync();
    Task<Plan?> GetPlanByIdAsync(int planId);
    Task<Plan?> GetUserPlanAsync(string userId);
}
