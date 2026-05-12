using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pixtract.Application.Interfaces;
using Pixtract.Domain.Entities;
using System.Security.Claims;

namespace Pixtract.Api.Controllers;

[ApiController]
[Route("api/plans")]
public class PlansApiController : ControllerBase
{
    private readonly IPricingService _pricingService;
    private readonly UserManager<ApplicationUser> _userManager;

    public PlansApiController(IPricingService pricingService, UserManager<ApplicationUser> userManager)
    {
        _pricingService = pricingService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var plans = await _pricingService.GetAllPlansAsync();
        return Ok(plans);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var plan = await _pricingService.GetPlanByIdAsync(id);
        if (plan == null) return NotFound();
        return Ok(plan);
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<IActionResult> GetUserPlanInfo()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.Users
            .Include(u => u.Plan)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound();

        int? daysRemaining = null;
        if (user.PlanExpiresAt.HasValue && user.PlanExpiresAt.Value > DateTime.UtcNow)
            daysRemaining = (int)Math.Ceiling((user.PlanExpiresAt.Value - DateTime.UtcNow).TotalDays);

        return Ok(new
        {
            planId = user.PlanId,
            planName = user.Plan?.Name ?? "Free",
            planExpiresAt = user.PlanExpiresAt,
            daysRemaining
        });
    }
}