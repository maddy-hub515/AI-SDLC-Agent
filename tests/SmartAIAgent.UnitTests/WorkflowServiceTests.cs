using FluentAssertions;
using SmartAIAgent.Application.Common;
using SmartAIAgent.Application.Models.AgentRuns;
using SmartAIAgent.Application.Services;
using SmartAIAgent.Domain.Entities;
using SmartAIAgent.Domain.Enums;
using SmartAIAgent.UnitTests.TestHelpers;

namespace SmartAIAgent.UnitTests;

public sealed class WorkflowServiceTests
{
    [Fact]
    public async Task StartAsync_ShouldDelegateToRequirementAgent()
    {
        var expected = new AgentRunDetailsDto
        {
            Id = Guid.NewGuid(),
            RequirementId = Guid.NewGuid(),
            Status = AgentRunStatus.WaitingForApproval,
            CurrentStage = AgentStage.AwaitingApproval,
            WorkflowEvents =
            [
                new() { Id = Guid.NewGuid(), EventType = "RequirementAnalysis", FromStage = AgentStage.None, ToStage = AgentStage.RequirementAnalysis, Message = "Requirement analysis started.", CreatedAtUtc = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), EventType = "UserStoryPersisted", FromStage = AgentStage.AiProcessing, ToStage = AgentStage.UserStoryPersisted, Message = "AI generated user story persisted.", CreatedAtUtc = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), EventType = "AwaitingApproval", FromStage = AgentStage.UserStoryPersisted, ToStage = AgentStage.AwaitingApproval, Message = "Workflow is waiting for human approval.", CreatedAtUtc = DateTime.UtcNow }
            ],
            Approvals =
            [
                new() { Id = Guid.NewGuid(), Status = ApprovalStatus.Pending, Type = ApprovalType.UserStory, CreatedAtUtc = DateTime.UtcNow }
            ]
        };

        await using var handle = TestDbContextFactory.Create();
        var service = new WorkflowService(
            handle.DbContext,
            new FakeDateTimeProvider(DateTime.UtcNow),
            new FakeRequirementAgent((requirementId, _) =>
            {
                expected.RequirementId.Should().Be(requirementId);
                return Task.FromResult(expected);
            }),
            TestLoggerFactory.Create<WorkflowService>());

        var result = await service.StartAsync(expected.RequirementId, CancellationToken.None);

        result.Status.Should().Be(AgentRunStatus.WaitingForApproval);
        result.CurrentStage.Should().Be(AgentStage.AwaitingApproval);
        result.WorkflowEvents.Should().HaveCount(3);
        result.Approvals.Should().ContainSingle(x => x.Status == ApprovalStatus.Pending);
    }

    [Fact]
    public async Task ApproveAsync_ShouldCompleteValidTransition()
    {
        await using var handle = TestDbContextFactory.Create();
        var dbContext = handle.DbContext;
        var now = new DateTime(2026, 8, 28, 0, 0, 0, DateTimeKind.Utc);
        var requirement = CreateRequirement(now);
        var run = await SeedAwaitingApprovalRunAsync(dbContext, requirement, now);

        var provider = new FakeDateTimeProvider(now);
        var service = new WorkflowService(dbContext, provider, new FakeRequirementAgent(), TestLoggerFactory.Create<WorkflowService>());

        provider.UtcNow = now.AddMinutes(5);
        var approved = await service.ApproveAsync(run.Id, "Looks good", CancellationToken.None);

        approved.Status.Should().Be(AgentRunStatus.Approved);
        approved.CurrentStage.Should().Be(AgentStage.Completed);
        approved.Approvals.Should().ContainSingle(x => x.Status == ApprovalStatus.Approved);
    }

    [Fact]
    public async Task ApproveAsync_ShouldFailForInvalidTransition()
    {
        await using var handle = TestDbContextFactory.Create();
        var dbContext = handle.DbContext;
        var now = DateTime.UtcNow;
        var requirement = CreateRequirement(now);
        var run = new AgentRun
        {
            Id = Guid.NewGuid(),
            RequirementId = requirement.Id,
            Requirement = requirement,
            CurrentStage = AgentStage.RequirementAnalysis,
            Status = AgentRunStatus.Running,
            StartedAtUtc = now,
            Approvals = new List<Approval>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Status = ApprovalStatus.Pending,
                    Type = ApprovalType.UserStory,
                    CreatedAtUtc = now
                }
            }
        };
        dbContext.Requirements.Add(requirement);
        dbContext.AgentRuns.Add(run);
        await dbContext.SaveChangesAsync();

        var service = new WorkflowService(dbContext, new FakeDateTimeProvider(now), new FakeRequirementAgent(), TestLoggerFactory.Create<WorkflowService>());

        var action = async () => await service.ApproveAsync(run.Id, null, CancellationToken.None);

        var exception = await action.Should().ThrowAsync<ApplicationError>();
        exception.Which.Code.Should().Be("INVALID_WORKFLOW_TRANSITION");
    }

    [Fact]
    public async Task RejectAsync_ShouldRejectValidTransition()
    {
        await using var handle = TestDbContextFactory.Create();
        var dbContext = handle.DbContext;
        var now = DateTime.UtcNow;
        var requirement = CreateRequirement(now);
        var run = await SeedAwaitingApprovalRunAsync(dbContext, requirement, now);

        var provider = new FakeDateTimeProvider(now);
        var service = new WorkflowService(dbContext, provider, new FakeRequirementAgent(), TestLoggerFactory.Create<WorkflowService>());

        provider.UtcNow = now.AddMinutes(2);
        var rejected = await service.RejectAsync(run.Id, "Needs more detail", CancellationToken.None);

        rejected.Status.Should().Be(AgentRunStatus.Rejected);
        rejected.Approvals.Should().ContainSingle(x => x.Status == ApprovalStatus.Rejected);
        rejected.ErrorMessage.Should().Be("Needs more detail");
    }

    [Fact]
    public async Task DuplicateApproval_ShouldFail()
    {
        await using var handle = TestDbContextFactory.Create();
        var dbContext = handle.DbContext;
        var now = DateTime.UtcNow;
        var requirement = CreateRequirement(now);
        var run = await SeedAwaitingApprovalRunAsync(dbContext, requirement, now);

        var provider = new FakeDateTimeProvider(now);
        var service = new WorkflowService(dbContext, provider, new FakeRequirementAgent(), TestLoggerFactory.Create<WorkflowService>());
        await service.ApproveAsync(run.Id, "Approved", CancellationToken.None);

        var action = async () => await service.ApproveAsync(run.Id, "Approved again", CancellationToken.None);

        var exception = await action.Should().ThrowAsync<ApplicationError>();
        exception.Which.Code.Should().Be("AGENT_RUN_ALREADY_APPROVED");
    }

    [Fact]
    public async Task ApprovingRejectedRun_ShouldFail()
    {
        await using var handle = TestDbContextFactory.Create();
        var dbContext = handle.DbContext;
        var now = DateTime.UtcNow;
        var requirement = CreateRequirement(now);
        var run = await SeedAwaitingApprovalRunAsync(dbContext, requirement, now);

        var provider = new FakeDateTimeProvider(now);
        var service = new WorkflowService(dbContext, provider, new FakeRequirementAgent(), TestLoggerFactory.Create<WorkflowService>());
        await service.RejectAsync(run.Id, "Reject", CancellationToken.None);

        var action = async () => await service.ApproveAsync(run.Id, null, CancellationToken.None);

        await action.Should().ThrowAsync<ApplicationError>();
    }

    [Fact]
    public async Task RejectingApprovedRun_ShouldFail()
    {
        await using var handle = TestDbContextFactory.Create();
        var dbContext = handle.DbContext;
        var now = DateTime.UtcNow;
        var requirement = CreateRequirement(now);
        var run = await SeedAwaitingApprovalRunAsync(dbContext, requirement, now);

        var provider = new FakeDateTimeProvider(now);
        var service = new WorkflowService(dbContext, provider, new FakeRequirementAgent(), TestLoggerFactory.Create<WorkflowService>());
        await service.ApproveAsync(run.Id, null, CancellationToken.None);

        var action = async () => await service.RejectAsync(run.Id, "Reject", CancellationToken.None);

        await action.Should().ThrowAsync<ApplicationError>();
    }

    private static Requirement CreateRequirement(DateTime now) => new()
    {
        Id = Guid.NewGuid(),
        Title = "Requirement A",
        Description = "Description A",
        Status = RequirementStatus.Submitted,
        CreatedAtUtc = now,
        UpdatedAtUtc = now
    };

    private static async Task<AgentRun> SeedAwaitingApprovalRunAsync(SmartAIAgent.Infrastructure.Persistence.SmartAIAgentDbContext dbContext, Requirement requirement, DateTime now)
    {
        var run = new AgentRun
        {
            Id = Guid.NewGuid(),
            RequirementId = requirement.Id,
            Requirement = requirement,
            CurrentStage = AgentStage.AwaitingApproval,
            Status = AgentRunStatus.WaitingForApproval,
            StartedAtUtc = now,
            Approvals = new List<Approval>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    AgentRunId = Guid.Empty,
                    Status = ApprovalStatus.Pending,
                    Type = ApprovalType.UserStory,
                    CreatedAtUtc = now
                }
            }
        };

        run.Approvals.Single().AgentRunId = run.Id;

        dbContext.Requirements.Add(requirement);
        dbContext.AgentRuns.Add(run);
        await dbContext.SaveChangesAsync();
        return run;
    }
}
