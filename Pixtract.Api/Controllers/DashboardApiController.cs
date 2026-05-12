using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pixtract.Application.Interfaces;
using System.Security.Claims;

namespace Pixtract.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardApiController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardApiController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        return Ok(new
        {
            TodayUsage = await _dashboardService.GetTodayUsageAsync(userId),
            DailyLimit = await _dashboardService.GetDailyLimitAsync(userId),
            PlanName = await _dashboardService.GetPlanNameAsync(userId),
            RecentExtractions = await _dashboardService.GetRecentExtractionsAsync(userId, 10)
        });
    }
}