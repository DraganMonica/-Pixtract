using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pixtract.Web.Client;

namespace Pixtract.Web.Controllers;

[Authorize]
public class PricingController : Controller
{
    private readonly ApiClient _apiClient;

    public PricingController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var plans = await _apiClient.GetAllPlansAsync();
        var (planName, daysRemaining, planId) = await _apiClient.GetUserPlanInfoAsync();
        ViewBag.CurrentPlanId = planId;
        ViewBag.CurrentPlanName = planName;
        ViewBag.DaysRemaining = daysRemaining;
        return View(plans);
    }
}