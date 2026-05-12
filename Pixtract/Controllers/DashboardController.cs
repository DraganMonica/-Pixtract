using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pixtract.Web.Client;

namespace Pixtract.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApiClient _apiClient;

    public DashboardController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.Name == "admin@pixtract.com")
            return RedirectToAction("Index", "Admin");

        var vm = await _apiClient.GetDashboardAsync();

        if (vm == null)
        {
            // API a picat sau tokenul e invalid — logout complet
            _apiClient.ClearToken();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["error"] = "Sesiunea a expirat. Te rugam sa te autentifici din nou.";
            return RedirectToAction("Login", "Account");
        }

        return View(vm);
    }
}