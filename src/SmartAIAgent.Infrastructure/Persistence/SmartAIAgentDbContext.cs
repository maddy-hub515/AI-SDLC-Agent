using Microsoft.EntityFrameworkCore;
using SmartAIAgent.Application.Interfaces;
using SmartAIAgent.Domain.Entities;

namespace SmartAIAgent.Infrastructure.Persistence;

public sealed class SmartAIAgentDbContext : DbContext, IApplicationDbContext
{
    public SmartAIAgentDbContext(DbContextOptions<SmartAIAgentDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Requirement> Requirements => Set<Requirement>();
    public DbSet<UserStory> UserStories => Set<UserStory>();
    public DbSet<UserStoryAcceptanceCriterion> UserStoryAcceptanceCriteria => Set<UserStoryAcceptanceCriterion>();
    public DbSet<AgentRun> AgentRuns => Set<AgentRun>();
    public DbSet<Approval> Approvals => Set<Approval>();
    public DbSet<WorkflowEvent> WorkflowEvents => Set<WorkflowEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartAIAgentDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
