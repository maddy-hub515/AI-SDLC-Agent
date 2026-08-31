using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAIAgent.Domain.Entities;

namespace SmartAIAgent.Infrastructure.Persistence.Configurations;

public sealed class RequirementConfiguration : IEntityTypeConfiguration<Requirement>
{
    public void Configure(EntityTypeBuilder<Requirement> builder)
    {
        builder.ToTable("Requirements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAtUtc);

        builder.HasMany(x => x.UserStories)
            .WithOne(x => x.Requirement)
            .HasForeignKey(x => x.RequirementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.AgentRuns)
            .WithOne(x => x.Requirement)
            .HasForeignKey(x => x.RequirementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
