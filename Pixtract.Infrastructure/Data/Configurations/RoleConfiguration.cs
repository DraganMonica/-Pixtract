using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Pixtract.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole { Id = "role-admin-id-0001", Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole { Id = "role-user-id-0002", Name = "User", NormalizedName = "USER" }
        );
    }
}