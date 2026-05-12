using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pixtract.Domain.Entities;

namespace Pixtract.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasOne(u => u.Plan)
            .WithMany()
            .HasForeignKey(u => u.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        var hasher = new PasswordHasher<ApplicationUser>();

        var adminUser = new ApplicationUser
        {
            Id = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
            UserName = "admin@pixtract.com",
            NormalizedUserName = "ADMIN@PIXTRACT.COM",
            Email = "admin@pixtract.com",
            NormalizedEmail = "ADMIN@PIXTRACT.COM",
            EmailConfirmed = true,
            PlanId = 3,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin123!");

        builder.HasData(adminUser);
    }
}