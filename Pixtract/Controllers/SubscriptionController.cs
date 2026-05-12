using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pixtract.Web.Client;

namespace Pixtract.Web.Controllers;

[Authorize]
public class SubscriptionController : Controller
{
    private readonly ApiClient _apiClient;

    public SubscriptionController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpPost]
    public async Task<IActionResult> Upgrade(int planId)
    {
        var checkoutUrl = await _apiClient.UpgradePlanAsync(planId);
        if (checkoutUrl == null)
        {
            TempData["Error"] = "Eroare la initierea platii.";
            return RedirectToAction("Index", "Pricing");
        }

        return Redirect(checkoutUrl);
    }

    [HttpGet]
    public async Task<IActionResult> Confirm(string session_id, int planId)
    {
        if (string.IsNullOrEmpty(session_id))
        {
            TempData["Error"] = "Session de plata invalida.";
            return RedirectToAction("Index", "Pricing");
        }

        var (success, error) = await _apiClient.ConfirmUpgradeAsync(session_id, planId);

        if (!success)
        {
            TempData["Error"] = error;
            return RedirectToAction("Index", "Pricing");
        }

        TempData["Success"] = "Plan actualizat cu succes!";
        return RedirectToAction("Index", "Dashboard");
    }
}