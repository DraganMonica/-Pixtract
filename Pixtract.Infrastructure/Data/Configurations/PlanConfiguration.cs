using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pixtract.Domain.Entities;

namespace Pixtract.Infrastructure.Data.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Price)
            .HasPrecision(10, 2);

        builder.HasData(
            new Plan { Id = 1, Name = "Free", DailyImageLimit = 5, ImagesPerRequest = 1, Price = 0, CanExportHistory = false },
            new Plan { Id = 2, Name = "Pro", DailyImageLimit = 50, ImagesPerRequest = 5, Price = 9.99m, CanExportHistory = true },
            new Plan { Id = 3, Name = "Ultra", DailyImageLimit = 200, ImagesPerRequest = 10, Price = 24.99m, CanExportHistory = true }
        );
    }
}