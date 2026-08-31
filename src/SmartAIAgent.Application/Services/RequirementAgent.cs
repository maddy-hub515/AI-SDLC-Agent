using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartAIAgent.Application.Common;
using SmartAIAgent.Application.Interfaces;
using SmartAIAgent.Application.Models.AgentRuns;
using SmartAIAgent.Application.Models.RequirementAnalysis;
using SmartAIAgent.Application.Options;
using SmartAIAgent.Domain.Entities;
using SmartAIAgent.Domain.Enums;

namespace SmartAIAgent.Application.Services;

public sealed class RequirementAgent : IRequirementAgent
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILlmService _llmService;
    private readonly IPromptService _promptService;
    private readonly IOptions<AiOptions> _aiOptions;
    private readonly IOptions<RequirementAnalysisOptions> _options;
    private readonly ILogger<RequirementAgent> _logger;

    public RequirementAgent(
        IApplicationDbContext dbContext,
        IDateTimeProvider dateTimeProvider,
        ILlmService llmService,
        IPromptService promptService,
        IOptions<AiOptions> aiOptions,
        IOptions<RequirementAnalysisOptions> options,
        ILogger<RequirementAgent> logger)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
        _llmService = llmService;
        _promptService = promptService;
        _aiOptions = aiOptions;
        _options = options;
        _logger = logger;
    }

    public async Task<AgentRunDetailsDto> AnalyzeAsync(Guid requirementId, CancellationToken cancellationToken)
    {
        var requirement = await _dbContext.Requirements
            .Include(x => x.AgentRuns)
            .Include(x => x.UserStories)
                .ThenInclude(x => x.AcceptanceCriteria)
            .FirstOrDefaultAsync(x => x.Id == requirementId, cancellationToken);

        if (requirement is null)
        {
            throw new ApplicationError("REQUIREMENT_NOT_FOUND", "Requirement was not found.");
        }

        var activeRunExists = requirement.AgentRuns.Any(x => x.Status is AgentRunStatus.Created or AgentRunStatus.Running or AgentRunStatus.WaitingForApproval);
        if (activeRunExists)
        {
            throw new ApplicationError("AGENT_RUN_ALREADY_ACTIVE", "An active agent run already exists for this requirement.");
        }

        var now = _dateTimeProvider.UtcNow;
        requirement.Status = RequirementStatus.Processing;
        requirement.UpdatedAtUtc = now;

        var prompt = await _promptService.GetRequirementAnalysisPromptAsync(requirement.Title, requirement.Description, cancellationToken);
        var agentRun = new AgentRun
        {
            Id = Guid.NewGuid(),
            RequirementId = requirementId,
            Status = AgentRunStatus.Created,
            CurrentStage = AgentStage.None,
            Provider = _aiOptions.Value.Provider,
            Model = _aiOptions.Value.Model,
            PromptVersion = prompt.Version,
            RetryCount = 0,
            StartedAtUtc = now
        };

        var pendingApproval = new Approval
        {
            Id = Guid.NewGuid(),
            AgentRunId = agentRun.Id,
            Type = ApprovalType.UserStory,
            Status = ApprovalStatus.Pending,
            CreatedAtUtc = now
        };

        var events = new List<WorkflowEvent>();
        _dbContext.AgentRuns.Add(agentRun);
        _dbContext.Approvals.Add(pendingApproval);

        Advance(agentRun, events, AgentStage.RequirementAnalysis, AgentRunStatus.Running, "RequirementAnalysis", "Requirement analysis started.", now);
        Advance(agentRun, events, AgentStage.UserStoryGeneration, AgentRunStatus.Running, "UserStoryGeneration", "User story generation started.", now);
        Advance(agentRun, events, AgentStage.AiProcessing, AgentRunStatus.Running, "AiProcessing", "AI processing started.", now);

        _dbContext.WorkflowEvents.AddRange(events);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Requirement agent started for requirement {RequirementId} with run {AgentRunId} and prompt {PromptVersion}",
            requirement.Id,
            agentRun.Id,
            prompt.Version);

        var maxRetries = _options.Value.MaxAutomaticRetries;
        for (var attempt = 0; attempt <= maxRetries; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                agentRun.RetryCount = attempt;

                var analysis = await _llmService.GenerateStructuredAsync<RequirementAnalysisResult>(
                    prompt.SystemPrompt,
                    prompt.UserPrompt,
                    cancellationToken);

                RequirementAnalysisValidator.ValidateAndNormalize(analysis, _options.Value);

                PersistUserStory(requirement, agentRun, analysis, _dateTimeProvider.UtcNow);
                await MoveToAwaitingApprovalAsync(requirement, agentRun, cancellationToken);

                _logger.LogInformation(
                    "Requirement agent completed successfully for requirement {RequirementId} and run {AgentRunId} after {RetryCount} retries",
                    requirement.Id,
                    agentRun.Id,
                    agentRun.RetryCount);

                return await LoadRunDetailsAsync(agentRun.Id, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                await FailRunAsync(requirement, agentRun, "Requirement analysis was cancelled.", CancellationToken.None);
                throw;
            }
            catch (Exception exception) when (exception is LlmException or HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning(
                    exception,
                    "Requirement agent attempt {Attempt} failed for requirement {RequirementId} and run {AgentRunId}",
                    attempt + 1,
                    requirement.Id,
                    agentRun.Id);

                if (attempt == maxRetries)
                {
                    await FailRunAsync(requirement, agentRun, "Requirement analysis failed after retrying the AI request.", cancellationToken);
                    throw new ApplicationError("REQUIREMENT_ANALYSIS_FAILED", "Requirement analysis failed. Please try again later.");
                }

                _dbContext.WorkflowEvents.Add(new WorkflowEvent
                {
                    Id = Guid.NewGuid(),
                    AgentRunId = agentRun.Id,
                    FromStage = agentRun.CurrentStage,
                    ToStage = agentRun.CurrentStage,
                    EventType = "Retry",
                    Message = $"Retry {attempt + 1} triggered after AI processing failure.",
                    CreatedAtUtc = _dateTimeProvider.UtcNow
                });

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        throw new ApplicationError("REQUIREMENT_ANALYSIS_FAILED", "Requirement analysis failed. Please try again later.");
    }

    private void PersistUserStory(Requirement requirement, AgentRun agentRun, RequirementAnalysisResult analysis, DateTime createdAtUtc)
    {
        var userStory = new UserStory
        {
            Id = Guid.NewGuid(),
            RequirementId = requirement.Id,
            AgentRunId = agentRun.Id,
            Title = analysis.UserStory!.Title.Trim(),
            Description = analysis.UserStory.Description.Trim(),
            TechnicalAreasJson = StructuredDataSerializer.SerializeCollection(analysis.TechnicalAreas),
            DevelopmentTasksJson = StructuredDataSerializer.SerializeCollection(analysis.DevelopmentTasks),
            AssumptionsJson = StructuredDataSerializer.SerializeCollection(analysis.Assumptions),
            CreatedAtUtc = createdAtUtc,
            AcceptanceCriteria = analysis.AcceptanceCriteria
                .Select(x => new UserStoryAcceptanceCriterion
                {
                    Id = Guid.NewGuid(),
                    Value = x
                })
                .ToList()
        };

        _dbContext.UserStories.Add(userStory);
    }

    private async Task MoveToAwaitingApprovalAsync(Requirement requirement, AgentRun agentRun, CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.UtcNow;
        var events = new List<WorkflowEvent>();
        Advance(agentRun, events, AgentStage.UserStoryPersisted, AgentRunStatus.Running, "UserStoryPersisted", "AI generated user story persisted.", now);
        Advance(agentRun, events, AgentStage.AwaitingApproval, AgentRunStatus.WaitingForApproval, "AwaitingApproval", "Workflow is waiting for human approval.", now);
        _dbContext.WorkflowEvents.AddRange(events);

        requirement.Status = RequirementStatus.AwaitingApproval;
        requirement.UpdatedAtUtc = now;
        agentRun.CompletedAtUtc = now;
        agentRun.ErrorMessage = null;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task FailRunAsync(Requirement requirement, AgentRun agentRun, string safeMessage, CancellationToken cancellationToken)
    {
        var message = safeMessage.Length > _options.Value.ErrorMessageMaxLength
            ? safeMessage[.._options.Value.ErrorMessageMaxLength]
            : safeMessage;

        var now = _dateTimeProvider.UtcNow;
        requirement.Status = RequirementStatus.Failed;
        requirement.UpdatedAtUtc = now;
        agentRun.Status = AgentRunStatus.Failed;
        agentRun.CompletedAtUtc = now;
        agentRun.ErrorMessage = message;

        _dbContext.WorkflowEvents.Add(new WorkflowEvent
        {
            Id = Guid.NewGuid(),
            AgentRunId = agentRun.Id,
            FromStage = agentRun.CurrentStage,
            ToStage = agentRun.CurrentStage,
            EventType = "Failed",
            Message = message,
            CreatedAtUtc = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<AgentRunDetailsDto> LoadRunDetailsAsync(Guid agentRunId, CancellationToken cancellationToken)
    {
        var agentRun = await _dbContext.AgentRuns
            .AsNoTracking()
            .Include(x => x.Approvals)
            .Include(x => x.WorkflowEvents)
            .FirstAsync(x => x.Id == agentRunId, cancellationToken);

        return AgentRunMapper.MapDetails(agentRun);
    }

    private void Advance(AgentRun agentRun, ICollection<WorkflowEvent> events, AgentStage nextStage, AgentRunStatus nextStatus, string eventType, string message, DateTime occurredAtUtc)
    {
        var fromStage = agentRun.CurrentStage;
        agentRun.CurrentStage = nextStage;
        agentRun.Status = nextStatus;

        events.Add(new WorkflowEvent
        {
            Id = Guid.NewGuid(),
            AgentRunId = agentRun.Id,
            FromStage = fromStage,
            ToStage = nextStage,
            EventType = eventType,
            Message = message,
            CreatedAtUtc = occurredAtUtc
        });

        _logger.LogInformation(
            "Agent run {AgentRunId} transitioned from {FromStage} to {ToStage}",
            agentRun.Id,
            fromStage,
            nextStage);
    }
}
