using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pixtract.Domain.Entities;

namespace Pixtract.Infrastructure.Data.Configurations;

public class UserDailyUsageConfiguration : IEntityTypeConfiguration<UserDailyUsage>
{
    public void Configure(EntityTypeBuilder<UserDailyUsage> builder)
    {
        builder.HasKey(u => u.Id);

        builder.HasIndex(u => new { u.UserId, u.Date })
            .IsUnique();

        builder.HasOne(u => u.User)
            .WithMany()
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}