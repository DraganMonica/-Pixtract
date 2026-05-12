using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pixtract.Application.DTOs;
using Pixtract.Web.Client;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Pixtract.Web.Controllers;

public class AccountController : Controller
{
    private readonly ApiClient _apiClient;

    public AccountController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            TempData.Remove("error");
            TempData["success"] = "Autentificare reusita!";
            return RedirectToAction("Index", "Dashboard");
        }
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["error"] = "Datele introduse nu sunt valide.";
            return View(dto);
        }

        var (success, error) = await _apiClient.LoginAsync(dto.Email, dto.Password);

        if (!success)
        {
            TempData["error"] = error;
            ModelState.AddModelError(string.Empty, error);
            return View(dto);
        }

        var token = HttpContext.Session.GetString("jwt_token") ?? string.Empty;
        await SignInWithCookieAsync(dto.Email, token);

        TempData["success"] = "Autentificare reusita!";

        if (dto.Email == "admin@pixtract.com")
            return RedirectToAction("Index", "Admin");

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            TempData["info"] = "Esti deja autentificat.";
            return RedirectToAction("Index", "Dashboard");
        }
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["error"] = "Completeaza corect toate campurile.";
            return View(dto);
        }

        var (success, error) = await _apiClient.RegisterAsync(dto);

        if (!success)
        {
            TempData["error"] = error;
            ModelState.AddModelError(string.Empty, error);
            return View(dto);
        }

        TempData["success"] = "Cont creat cu succes! Te poti autentifica.";
        return RedirectToAction("Login");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        _apiClient.ClearToken();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        TempData["success"] = "Te-ai delogat cu succes.";
        return RedirectToAction("Login", "Account");
    }

    private async Task SignInWithCookieAsync(string email, string jwtToken)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Email, email),
            new Claim("jwt_token", jwtToken),
        };

        var handler = new JwtSecurityTokenHandler();
        if (handler.CanReadToken(jwtToken))
        {
            var jwt = handler.ReadJwtToken(jwtToken);
            foreach (var c in jwt.Claims.Where(c =>
                c.Type == "role" ||
                c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"))
                claims.Add(new Claim(ClaimTypes.Role, c.Value));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            });
    }
}