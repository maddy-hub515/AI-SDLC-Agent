using FluentAssertions;
using SmartAIAgent.Application.Services;
using SmartAIAgent.Domain.Entities;
using SmartAIAgent.Domain.Enums;
using SmartAIAgent.UnitTests.TestHelpers;

namespace SmartAIAgent.UnitTests;

public sealed class ApprovalServiceTests
{
    [Fact]
    public async Task GetByAgentRunIdAsync_ShouldReturnApprovals()
    {
        await using var handle = TestDbContextFactory.Create();
        var dbContext = handle.DbContext;
        var requirementId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        dbContext.Requirements.Add(new Requirement
        {
            Id = requirementId,
            Title = "Requirement A",
            Description = "Description A",
            Status = RequirementStatus.Submitted,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        });
        dbContext.AgentRuns.Add(new AgentRun
        {
            Id = runId,
            RequirementId = requirementId,
            Status = AgentRunStatus.WaitingForApproval,
            CurrentStage = AgentStage.AwaitingApproval,
            StartedAtUtc = DateTime.UtcNow
        });
        dbContext.Approvals.Add(new Approval
        {
            Id = Guid.NewGuid(),
            AgentRunId = runId,
            Type = ApprovalType.UserStory,
            Status = ApprovalStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var service = new ApprovalService(dbContext);
        var result = await service.GetByAgentRunIdAsync(runId, CancellationToken.None);

        result.Should().ContainSingle();
    }
}
