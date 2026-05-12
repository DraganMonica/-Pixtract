using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pixtract.Application.Interfaces;
using System.Security.Claims;

namespace Pixtract.Api.Controllers;

[ApiController]
[Route("api/usage")]
[Authorize]
public class UsageApiController : ControllerBase
{
    private readonly IUsageService _usageService;

    public UsageApiController(IUsageService usageService)
    {
        _usageService = usageService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        return Ok(new
        {
            TodayUsage = await _usageService.GetTodayUsageAsync(userId),
            CanProcess = await _usageService.CanProcessAsync(userId),
            ImagesPerRequest = await _usageService.GetImagesPerRequestAsync(userId)
        });
    }
}