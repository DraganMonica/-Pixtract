using Microsoft.AspNetCore.Mvc;
using Pixtract.Application.DTOs;
using Pixtract.Application.Interfaces;

namespace Pixtract.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, error) = await _authService.RegisterAsync(dto);

        if (!success)
            return BadRequest(new { error });

        return Ok(new { message = "Cont creat cu succes." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, token, error) = await _authService.LoginAsync(dto.Email, dto.Password);

        if (!success)
            return Unauthorized(new { error });

        return Ok(new { token });
    }
}