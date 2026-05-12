using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pixtract.Web.Client;

namespace Pixtract.Web.Controllers;

[Authorize]
public class ExportController : Controller
{
    private readonly ApiClient _apiClient;

    public ExportController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> ExportCurrent(int id)
    {
        var bytes = await _apiClient.ExportCurrentAsync(id);
        if (bytes == null)
            return NotFound();

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"extractie_{id}_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    public async Task<IActionResult> ExportHistory()
    {
        var bytes = await _apiClient.ExportHistoryAsync();
        if (bytes == null || bytes.Length == 0)
            return RedirectToAction("Index", "Dashboard");

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"istoric_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }
}