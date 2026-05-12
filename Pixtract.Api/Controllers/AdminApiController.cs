using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pixtract.Application.Interfaces;

namespace Pixtract.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminApiController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminApiController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(new
        {
            Users = await _adminService.GetAllUsersAsync(),
            TotalExtractions = await _adminService.GetTotalExtractionsAsync(),
            TodayExtractions = await _adminService.GetTodayExtractionsAsync(),
            ActiveUsersToday = await _adminService.GetActiveUsersTodayAsync(),
            ExpiringSoon = await _adminService.GetExpiringSoonAsync(),
            TopCategories = await _adminService.GetTopCategoriesAsync()
        });
    }
}