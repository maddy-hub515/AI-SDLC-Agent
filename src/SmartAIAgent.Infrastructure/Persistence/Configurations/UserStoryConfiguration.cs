using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAIAgent.Domain.Entities;

namespace SmartAIAgent.Infrastructure.Persistence.Configurations;

public sealed class UserStoryConfiguration : IEntityTypeConfiguration<UserStory>
{
    public void Configure(EntityTypeBuilder<UserStory> builder)
    {
        builder.ToTable("UserStories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AgentRunId);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.TechnicalAreasJson).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.DevelopmentTasksJson).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.AssumptionsJson).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.RequirementId);
        builder.HasIndex(x => x.AgentRunId);

        builder.HasOne(x => x.AgentRun)
            .WithMany(x => x.UserStories)
            .HasForeignKey(x => x.AgentRunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.AcceptanceCriteria)
            .WithOne(x => x.UserStory)
            .HasForeignKey(x => x.UserStoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
