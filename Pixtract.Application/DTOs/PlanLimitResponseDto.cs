namespace Pixtract.Application.DTOs;

public class PlanLimitResponseDto
{
    public bool LimitReached { get; set; }
    public string Message { get; set; }
    public int CurrentUsage { get; set; }
    public int DailyLimit { get; set; }
    public string CurrentPlan { get; set; }
    public List<PlanDto> AvailableUpgrades { get; set; } = new();
}
