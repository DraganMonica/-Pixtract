using Pixtract.Application.DTOs;

namespace Pixtract.Web.VMs;

public class DashboardVM
{
    public int TodayUsage { get; set; }
    public int DailyLimit { get; set; }
    public string PlanName { get; set; }
    public List<ExtractionDto> RecentExtractions { get; set; } = new();

    // calculat automat
    public int RemainingImages => DailyLimit - TodayUsage;
    public int UsagePercentage => DailyLimit > 0
        ? (int)((TodayUsage * 100.0) / DailyLimit)
        : 0;
}
