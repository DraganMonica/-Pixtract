using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pixtract.Application.Interfaces;
using Pixtract.Domain.Entities;
using Pixtract.Infrastructure.Data;
using Stripe.Checkout;

namespace Pixtract.Infrastructure.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
        IConfiguration configuration, ILogger<SubscriptionService> logger)
    {
        // folosesc db pentru a lucra cu planurile din baza de date
        _db = db;
        _userManager = userManager;
        // folosesc configuration pentru a citi cheile si url urile din appsettings.json
        _configuration = configuration;
        _logger = logger;
    }

    //creez sesiunea Stripe Checkout pentru upgrade ul planului + 
    public async Task<string> CreateFakeCheckoutAsync(string userId, int planId)
    {

        //PAS 1 iau adresa principala a aplicatiei mele web din configuratie
        var webBaseUrl = _configuration["Stripe:WebBaseUrl"] ?? "https://localhost:7284";

        // PAS 2  caut planul selectat in db + verific daca user ul a ales planul Free
        var plan = await _db.Plans.FirstOrDefaultAsync(p => p.Id == planId);
        if (plan != null && plan.Price == 0)
        {
            _logger.LogInformation("Downgrade la Free pentru user ul: {UserId}.", userId);
            await UpgradePlanAsync(userId, planId);
            return $"{webBaseUrl}/Dashboard";
        }

        //PAS 3 aleg PriceId ul Stripe in functie de plan + verificare daca e valid
        var priceId = planId switch
        {
            2 => _configuration["Stripe:ProPriceId"],
            3 => _configuration["Stripe:UltraPriceId"],
            _ => null
        };
        if (string.IsNullOrEmpty(priceId))
        {
            _logger.LogWarning("PlanId invalid sau fara pret Stripe asociat. UserId={UserId}, PlanId={PlanId}", userId, planId);
            return "/Pricing";
        }
        //PAS 4 configurez sesiunea Stripe Checkout
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            // adaug produsul si cantitatea(la noi e 1, ca e vorba de un abonament)
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions { Price = priceId, Quantity = 1 }
            },
            Mode = "subscription",
            //url daca fac abonamentul -  localhost ul/endpoint ul care confirma abonamentul/placeholder special Stripe; Stripe inlocuieste automat cu id ul real al sesiunii de plata
            SuccessUrl = $"{webBaseUrl}/Subscription/Confirm?session_id={{CHECKOUT_SESSION_ID}}&planId={planId}",
            CancelUrl = $"{webBaseUrl}/Pricing",
            Metadata = new Dictionary<string, string>
            {
                { "userId", userId },
                { "planId", planId.ToString() }
            }
        };

        _logger.LogInformation("Sesiune Stripe creata pentru user ul: {UserId}. PlanId={PlanId}, PriceId={PriceId}", userId, planId, priceId);

        // PAS 5 Creez  efectiv sesiunea Stripe
        var service = new SessionService();
        var session = await service.CreateAsync(options);

        _logger.LogInformation("User ul: {UserId} redirectionat catre Stripe Checkout. SessionId={SessionId}", userId, session.Id);
        return session.Url;
    }

    //actualizarea planului user ului
    public async Task<(bool Success, string Error)> UpgradePlanAsync(string userId, int planId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Upgrade esuat. User ul nu a fost gasit. UserId={UserId}", userId);
            return (false, "User negasit");
        }

        //folosesc asta, pentru log ul de ma tarziu(ca s a trecut de la un plan la altul)
        var planVechi = user.PlanId;
        //actualizez planul utilizatorului, apoi caut nou plan in db
        user.PlanId = planId;
        var plan = await _db.Plans.FirstOrDefaultAsync(p => p.Id == planId);

        //setez data expirarii
        user.PlanExpiresAt = (plan != null && plan.Price > 0)
            ? DateTime.UtcNow.AddDays(30)
            : null;

        //actualizez user ul
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Eroare la actualizarea planului pentru user ul: {UserId}. Erori={Errors}", userId, errors);
            return (false, "Eroare la actualizarea planului.");
        }

        _logger.LogInformation("Plan actualizat cu succes pentru user ul: {UserId}. PlanVechi={PlanVechi}, PlanNou={PlanNou}, Expira={ExpiresAt}",
            userId, planVechi, planId, user.PlanExpiresAt);
        return (true, string.Empty);
    }
}