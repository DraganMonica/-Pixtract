using Microsoft.AspNetCore.Identity;

namespace Pixtract.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    //email, password, id, deja se regases din IdentityUser(de aceea o mosteneste)
    public int PlanId { get; set; }
    public Plan Plan { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PlanExpiresAt { get; set; }
}
