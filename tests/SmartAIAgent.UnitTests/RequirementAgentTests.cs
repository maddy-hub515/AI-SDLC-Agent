using FluentAssertions;
using Microsoft.Extensions.Options;
using SmartAIAgent.Application.Common;
using SmartAIAgent.Application.Models.RequirementAnalysis;
using SmartAIAgent.Application.Options;
using SmartAIAgent.Application.Services;
using SmartAIAgent.Domain.Entities;
using SmartAIAgent.Domain.Enums;
using SmartAIAgent.UnitTests.TestHelpers;

namespace SmartAIAgent.UnitTests;

public sealed class RequirementAgentTests
{
    private static readonly RequirementAnalysisResult ValidResult = new()
    {
        UserStory = new RequirementAnalysisUserStoryResult
        {
            Title = "Remove Outcode Restriction from Linked Case Assignment",
            Description = "As a case management system, I want linked case assignment to consider eligible officers regardless of outcode so that valid officers are not excluded by the outcode restriction."
        },
        AcceptanceCriteria =
        [
            "Linked case assignment must not restrict eligible officers based on outcode.",
            "All other officer eligibility rules must remain unchanged.",
            "Existing assignment behavior outside the changed rule must remain unaffected."
        ],
        TechnicalAreas =
        [
            "Linked Case Assignment",
            "Officer Eligibility"
        ],
        DevelopmentTasks =
        [
            "Analyze the existing linked case assignment logic.",
            "Remove the outcode restriction.",
            "Add regression coverage for officers with different outcodes."
        ],
        Assumptions =
        [
            "Existing officer eligibility rules remain unchanged."
        ]
    };

    [Fact]
    public async Task AnalyzeAsync_ShouldPersistAnalysisAndMoveToAwaitingApproval()
    {
        await using var handle = TestDbContextFactory.Create();
        var dbContext = handle.DbContext;
        var now = new DateTime(2026, 8, 28, 0, 0, 0, DateTimeKind.Utc);
        dbContext.Requirements.Add(new Requirement
        {
            Id = Guid.NewGuid(),
            Title = "Remove outcode restriction",
            Description = "Linked case assignment should ignore officer outcode.",
            Status = RequirementStatus.Submitted,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });
        await dbContext.SaveChangesAsync();

        var requirementId = dbContext.Requirements.Single().Id;
        var agent = CreateAgent(
            dbContext,
            new FakeDateTimeProvider(now),
            new FakeLlmService((_, _, _) => Task.FromResult<object>(CloneValidResult())));

        var result = await agent.AnalyzeAsync(requirementId, CancellationToken.None);

        result.Status.Should().Be(AgentRunStatus.WaitingForApproval);
        result.CurrentStage.Should().Be(AgentStage.AwaitingApproval);
        result.PromptVersion.Should().Be("Requirement.UserStory.v1");
        result.WorkflowEvents.Should().Contain(x => x.EventType == "UserStoryPersisted");

        dbContext.UserStories.Should().ContainSingle();
        dbContext.UserStories.Single().Title.Should().Be(ValidResult.UserStory!.Title);
        dbContext.Approvals.Should().ContainSingle(x => x.Status == ApprovalStatus.Pending);
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldRejectInvalidResponse()
    {
        await using var handle = TestDbContextFactory.Create();
        var dbContext = handle.DbContext;
        var now = DateTime.UtcNow;
        dbContext.Requirements.Add(new Requirement
        {
            Id = Guid.NewGuid(),
            Title = "Requirement A",
            Description = "Description A",
            Status = RequirementStatus.Submitted,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });
        await dbContext.SaveChangesAsync();

        var agent = CreateAgent(
            dbContext,
            new FakeDateTimeProvider(now),
            new FakeLlmService((_, _, _) => Task.FromResult<object>(new RequirementAnalysisResult())));

        var action = async () => await agent.AnalyzeAsync(dbContext.Requirements.Single().Id, CancellationToken.None);

        var exception = await action.Should().ThrowAsync<ApplicationError>();
        exception.Which.Code.Should().Be("REQUIREMENT_ANALYSIS_FAILED");
        dbContext.AgentRuns.Single().Status.Should().Be(AgentRunStatus.Failed);
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldRetryUntilSuccess()
    {
        await using var handle = TestDbContextFactory.Create();
        var dbContext = handle.DbContext;
        var now = DateTime.UtcNow;
        dbContext.Requirements.Add(new Requirement
        {
            Id = Guid.NewGuid(),
            Title = "Requirement A",
            Description = "Description A",
            Status = RequirementStatus.Submitted,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });
        await dbContext.SaveChangesAsync();

        var attempt = 0;
        var agent = CreateAgent(
            dbContext,
            new FakeDateTimeProvider(now),
            new FakeLlmService((_, _, _) =>
            {
                attempt++;
                if (attempt == 1)
                {
                    throw new LlmException("bad json");
                }

                return Task.FromResult<object>(CloneValidResult());
            }));

        var result = await agent.AnalyzeAsync(dbContext.Requirements.Single().Id, CancellationToken.None);

        attempt.Should().Be(2);
        result.RetryCount.Should().Be(1);
        result.Status.Should().Be(AgentRunStatus.WaitingForApproval);
        result.WorkflowEvents.Should().Contain(x => x.EventType == "Retry");
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldRespectRetryLimitForConnectionFailures()
    {
        await using var handle = TestDbContextFactory.Create();
        var dbContext = handle.DbContext;
        var now = DateTime.UtcNow;
        dbContext.Requirements.Add(new Requirement
        {
            Id = Guid.NewGuid(),
            Title = "Requirement A",
            Description = "Description A",
            Status = RequirementStatus.Submitted,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });
        await dbContext.SaveChangesAsync();

        var attempt = 0;
        var agent = CreateAgent(
            dbContext,
            new FakeDateTimeProvider(now),
            new FakeLlmService((_, _, _) =>
            {
                attempt++;
                throw new HttpRequestException("offline");
            }));

        var action = async () => await agent.AnalyzeAsync(dbContext.Requirements.Single().Id, CancellationToken.None);

        var exception = await action.Should().ThrowAsync<ApplicationError>();
        exception.Which.Code.Should().Be("REQUIREMENT_ANALYSIS_FAILED");
        attempt.Should().Be(3);
        dbContext.AgentRuns.Single().Status.Should().Be(AgentRunStatus.Failed);
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldRespectCancellation()
    {
        await using var handle = TestDbContextFactory.Create();
        var dbContext = handle.DbContext;
        var now = DateTime.UtcNow;
        dbContext.Requirements.Add(new Requirement
        {
            Id = Guid.NewGuid(),
            Title = "Requirement A",
            Description = "Description A",
            Status = RequirementStatus.Submitted,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });
        await dbContext.SaveChangesAsync();

        using var cancellationTokenSource = new CancellationTokenSource();

        var agent = CreateAgent(
            dbContext,
            new FakeDateTimeProvider(now),
            new FakeLlmService((_, _, token) =>
            {
                cancellationTokenSource.Cancel();
                token.ThrowIfCancellationRequested();
                return Task.FromResult<object>(CloneValidResult());
            }));

        var action = async () => await agent.AnalyzeAsync(dbContext.Requirements.Single().Id, cancellationTokenSource.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
        dbContext.AgentRuns.Single().Status.Should().Be(AgentRunStatus.Failed);
    }

    private static RequirementAgent CreateAgent(SmartAIAgent.Infrastructure.Persistence.SmartAIAgentDbContext dbContext, FakeDateTimeProvider dateTimeProvider, FakeLlmService llmService)
    {
        return new RequirementAgent(
            dbContext,
            dateTimeProvider,
            llmService,
            new FakePromptService(),
            Options.Create(new AiOptions
            {
                Provider = "Ollama",
                BaseUrl = "http://localhost:11434",
                Model = "test-model",
                TimeoutSeconds = 120,
                Temperature = 0.2m
            }),
            Options.Create(new RequirementAnalysisOptions()),
            TestLoggerFactory.Create<RequirementAgent>());
    }

    private static RequirementAnalysisResult CloneValidResult()
    {
        return new RequirementAnalysisResult
        {
            UserStory = new RequirementAnalysisUserStoryResult
            {
                Title = ValidResult.UserStory!.Title,
                Description = ValidResult.UserStory.Description
            },
            AcceptanceCriteria = [.. ValidResult.AcceptanceCriteria],
            TechnicalAreas = [.. ValidResult.TechnicalAreas],
            DevelopmentTasks = [.. ValidResult.DevelopmentTasks],
            Assumptions = [.. ValidResult.Assumptions]
        };
    }
}
