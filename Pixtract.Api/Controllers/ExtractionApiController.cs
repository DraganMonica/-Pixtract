using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pixtract.Application.DTOs;
using Pixtract.Application.Interfaces;
using System.Security.Claims;

namespace Pixtract.Api.Controllers;

[ApiController]
[Route("api/extraction")]
[Authorize]
public class ExtractionApiController : ControllerBase
{
    private readonly IExtractionService _extractionService;

    public ExtractionApiController(IExtractionService extractionService)
    {
        _extractionService = extractionService;
    }

    [HttpPost]
    public async Task<IActionResult> Extract([FromForm] string category, IFormFileCollection files)
    {
        if (files == null || files.Count == 0)
            return BadRequest(new { error = "Nicio imagine selectata." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var dtos = new List<ExtractionRequestDto>();
        foreach (var file in files)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            dtos.Add(new ExtractionRequestDto
            {
                Category = category,
                ImageBytes = ms.ToArray(),
                FileName = file.FileName,
                ContentType = file.ContentType
            });
        }

        var results = await _extractionService.ProcessMultipleAsync(userId, dtos);

        // Eroare de plan (un singur rezultat negativ)
        if (results.Count == 1 && !results[0].Success)
            return BadRequest(new { error = results[0].Error });

        return Ok(results);
    }

    [HttpGet("history")]
    public async Task<IActionResult> History()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var history = await _extractionService.GetHistoryAsync(userId);
        return Ok(history);
    }
}