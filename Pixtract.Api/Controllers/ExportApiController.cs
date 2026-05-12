using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pixtract.Application.Interfaces;
using System.Security.Claims;

namespace Pixtract.Api.Controllers;

[ApiController]
[Route("api/export")]
[Authorize]
public class ExportApiController : ControllerBase
{
    private readonly IExportService _exportService;
    private readonly IExtractionService _extractionService;

    public ExportApiController(IExportService exportService, IExtractionService extractionService)
    {
        _exportService = exportService;
        _extractionService = extractionService;
    }

    [HttpGet("current/{id}")]
    public async Task<IActionResult> ExportCurrent(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var request = await _extractionService.GetByIdAsync(id, userId);

        if (request == null) return NotFound();

        var bytes = _exportService.ExportCurrent(request);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"extractie_{request.ProductId}_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet("history")]
    public async Task<IActionResult> ExportHistory()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var history = await _extractionService.GetHistoryAsync(userId);

        if (!history.Any())
            return BadRequest(new { error = "Nu exista extractii de exportat." });

        var bytes = _exportService.ExportHistory(history);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"istoric_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }
}