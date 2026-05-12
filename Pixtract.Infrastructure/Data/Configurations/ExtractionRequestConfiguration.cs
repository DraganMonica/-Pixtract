using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pixtract.Domain.Entities;

namespace Pixtract.Infrastructure.Data.Configurations;

public class ExtractionRequestConfiguration : IEntityTypeConfiguration<ExtractionRequest>
{
    public void Configure(EntityTypeBuilder<ExtractionRequest> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.ResultJson)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}