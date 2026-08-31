using Microsoft.EntityFrameworkCore;
using SmartAIAgent.Domain.Entities;

namespace SmartAIAgent.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Project> Projects { get; }
    DbSet<Requirement> Requirements { get; }
    DbSet<UserStory> UserStories { get; }
    DbSet<UserStoryAcceptanceCriterion> UserStoryAcceptanceCriteria { get; }
    DbSet<AgentRun> AgentRuns { get; }
    DbSet<Approval> Approvals { get; }
    DbSet<WorkflowEvent> WorkflowEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
