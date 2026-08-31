using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAIAgent.Domain.Entities;

namespace SmartAIAgent.Infrastructure.Persistence.Configurations;

public sealed class AgentRunConfiguration : IEntityTypeConfiguration<AgentRun>
{
    public void Configure(EntityTypeBuilder<AgentRun> builder)
    {
        builder.ToTable("AgentRuns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Provider).HasMaxLength(100);
        builder.Property(x => x.Model).HasMaxLength(200);
        builder.Property(x => x.PromptVersion).HasMaxLength(100);
        builder.Property(x => x.RetryCount).IsRequired();
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        builder.Property(x => x.StartedAtUtc).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.CurrentStage).IsRequired();

        builder.HasIndex(x => x.RequirementId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CurrentStage);

        builder.HasMany(x => x.Approvals)
            .WithOne(x => x.AgentRun)
            .HasForeignKey(x => x.AgentRunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.WorkflowEvents)
            .WithOne(x => x.AgentRun)
            .HasForeignKey(x => x.AgentRunId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
