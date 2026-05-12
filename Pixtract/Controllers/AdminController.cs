using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pixtract.Web.Client;

namespace Pixtract.Web.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly ApiClient _apiClient;

    public AdminController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var vm = await _apiClient.GetAdminDataAsync();
        if (vm == null)
            return RedirectToAction("Login", "Account");

        return View(vm);
    }
}