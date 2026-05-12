using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pixtract.Application.DTOs;
using Pixtract.Application.Interfaces;
using Stripe.Checkout;
using System.Security.Claims;

namespace Pixtract.Api.Controllers;

[ApiController]
[Route("api/subscription")]
[Authorize]
public class SubscriptionApiController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionApiController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpPost("upgrade")]
    public async Task<IActionResult> Upgrade([FromBody] int planId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var checkoutUrl = await _subscriptionService.CreateFakeCheckoutAsync(userId, planId);
        return Ok(new { checkoutUrl });
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmUpgradeDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Verifică cu Stripe că plata e reală
        try
        {
            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(dto.SessionId);

            if (session.Status != "complete" && session.PaymentStatus != "paid")
                return BadRequest(new { error = "Plata nu a fost confirmata de Stripe." });
        }
        catch
        {
            return BadRequest(new { error = "Session Stripe invalida." });
        }

        var (success, error) = await _subscriptionService.UpgradePlanAsync(userId, dto.PlanId);

        if (!success)
            return BadRequest(new { error });

        return Ok(new { message = "Plan actualizat cu succes!" });
    }
}