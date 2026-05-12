using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pixtract.Web.Client;

namespace Pixtract.Web.Controllers;

[Authorize]
public class ExtractionController : Controller
{
    private readonly ApiClient _apiClient;

    public ExtractionController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var usage = await _apiClient.GetUsageAsync();
        ViewBag.MaxImages = usage.ImagesPerRequest;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Extract(List<IFormFile> files, string category)
    {
        if (files == null || !files.Any() || string.IsNullOrEmpty(category))
            return Json(new { success = false, error = "Fisiere sau categorie lipsa." });

        var results = await _apiClient.ExtractMultipleAsync(files, category);

        if (results == null)
            return Json(new { success = false, error = "Eroare la procesare. Verifica daca Colab ruleaza." });

        if (results.Count == 1 && !results[0].Success)
        {
            if (results[0].Error == "LIMIT_REACHED")
                return Json(new { limitReached = true });
            return Json(new { success = false, error = results[0].Error });
        }

        return Json(new { success = true, data = results });
    }
}