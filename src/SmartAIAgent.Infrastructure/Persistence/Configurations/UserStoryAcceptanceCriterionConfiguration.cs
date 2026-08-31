using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAIAgent.Domain.Entities;

namespace SmartAIAgent.Infrastructure.Persistence.Configurations;

public sealed class UserStoryAcceptanceCriterionConfiguration : IEntityTypeConfiguration<UserStoryAcceptanceCriterion>
{
    public void Configure(EntityTypeBuilder<UserStoryAcceptanceCriterion> builder)
    {
        builder.ToTable("UserStoryAcceptanceCriteria");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Value).HasMaxLength(500).IsRequired();
        builder.HasIndex(x => x.UserStoryId);
    }
}
