using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pixtract.Domain.Entities;

namespace Pixtract.Infrastructure.Data.Configurations;

public class BatchJobConfiguration : IEntityTypeConfiguration<BatchJob>
{
    public void Configure(EntityTypeBuilder<BatchJob> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}