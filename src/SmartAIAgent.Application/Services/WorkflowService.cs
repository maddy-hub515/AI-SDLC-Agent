using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartAIAgent.Application.Common;
using SmartAIAgent.Application.Interfaces;
using SmartAIAgent.Application.Models.AgentRuns;
using SmartAIAgent.Domain.Entities;
using SmartAIAgent.Domain.Enums;

namespace SmartAIAgent.Application.Services;

public sealed class WorkflowService : IWorkflowService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IRequirementAgent _requirementAgent;
    private readonly ILogger<WorkflowService> _logger;

    public WorkflowService(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider, IRequirementAgent requirementAgent, ILogger<WorkflowService> logger)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
        _requirementAgent = requirementAgent;
        _logger = logger;
    }

    public async Task<AgentRunDetailsDto> StartAsync(Guid requirementId, CancellationToken cancellationToken)
    {
        return await _requirementAgent.AnalyzeAsync(requirementId, cancellationToken);
    }

    public async Task<AgentRunDetailsDto> ApproveAsync(Guid agentRunId, string? comment, CancellationToken cancellationToken)
    {
        var agentRun = await LoadRunForDecisionAsync(agentRunId, cancellationToken);

        if (agentRun.Status == AgentRunStatus.Rejected)
        {
            throw new ApplicationError("AGENT_RUN_ALREADY_REJECTED", "Rejected runs cannot be approved.");
        }

        if (agentRun.Status == AgentRunStatus.Approved)
        {
            throw new ApplicationError("AGENT_RUN_ALREADY_APPROVED", "Agent run has already been approved.");
        }

        EnsureAwaitingApproval(agentRun);
        var pendingApproval = GetPendingApproval(agentRun);

        var now = _dateTimeProvider.UtcNow;
        pendingApproval.Status = ApprovalStatus.Approved;
        pendingApproval.Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        pendingApproval.DecidedAtUtc = now;

        agentRun.Status = AgentRunStatus.Approved;
        agentRun.CurrentStage = AgentStage.Completed;
        agentRun.CompletedAtUtc = now;
        agentRun.Requirement!.Status = RequirementStatus.Approved;
        agentRun.Requirement.UpdatedAtUtc = now;

        _dbContext.WorkflowEvents.Add(new WorkflowEvent
        {
            Id = Guid.NewGuid(),
            AgentRunId = agentRun.Id,
            FromStage = AgentStage.AwaitingApproval,
            ToStage = AgentStage.Completed,
            EventType = "Approved",
            Message = "Workflow approved by human reviewer.",
            CreatedAtUtc = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Agent run {AgentRunId} approved", agentRun.Id);

        return await GetRunAsync(agentRunId, cancellationToken);
    }

    public async Task<AgentRunDetailsDto> RejectAsync(Guid agentRunId, string reason, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ApplicationError("VALIDATION_ERROR", "Rejection reason is required.");
        }

        var agentRun = await LoadRunForDecisionAsync(agentRunId, cancellationToken);

        if (agentRun.Status == AgentRunStatus.Approved)
        {
            throw new ApplicationError("AGENT_RUN_ALREADY_APPROVED", "Approved runs cannot be rejected.");
        }

        if (agentRun.Status == AgentRunStatus.Rejected)
        {
            throw new ApplicationError("AGENT_RUN_ALREADY_REJECTED", "Agent run has already been rejected.");
        }

        EnsureAwaitingApproval(agentRun);
        var pendingApproval = GetPendingApproval(agentRun);

        var now = _dateTimeProvider.UtcNow;
        pendingApproval.Status = ApprovalStatus.Rejected;
        pendingApproval.Comment = reason.Trim();
        pendingApproval.DecidedAtUtc = now;

        agentRun.Status = AgentRunStatus.Rejected;
        agentRun.CompletedAtUtc = now;
        agentRun.ErrorMessage = reason.Trim();
        agentRun.Requirement!.Status = RequirementStatus.Rejected;
        agentRun.Requirement.UpdatedAtUtc = now;

        _dbContext.WorkflowEvents.Add(new WorkflowEvent
        {
            Id = Guid.NewGuid(),
            AgentRunId = agentRun.Id,
            FromStage = AgentStage.AwaitingApproval,
            ToStage = AgentStage.AwaitingApproval,
            EventType = "Rejected",
            Message = reason.Trim(),
            CreatedAtUtc = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Agent run {AgentRunId} rejected with reason {Reason}", agentRun.Id, reason.Trim());

        return await GetRunAsync(agentRunId, cancellationToken);
    }

    public async Task<AgentRunDetailsDto> GetRunAsync(Guid agentRunId, CancellationToken cancellationToken)
    {
        var agentRun = await _dbContext.AgentRuns
            .AsNoTracking()
            .Include(x => x.Approvals)
            .Include(x => x.WorkflowEvents)
            .FirstOrDefaultAsync(x => x.Id == agentRunId, cancellationToken);

        if (agentRun is null)
        {
            throw new ApplicationError("AGENT_RUN_NOT_FOUND", "Agent run was not found.");
        }

        return AgentRunMapper.MapDetails(agentRun);
    }

    private async Task<AgentRun> LoadRunForDecisionAsync(Guid agentRunId, CancellationToken cancellationToken)
    {
        var agentRun = await _dbContext.AgentRuns
            .Include(x => x.Requirement)
            .Include(x => x.Approvals)
            .FirstOrDefaultAsync(x => x.Id == agentRunId, cancellationToken);

        if (agentRun is null)
        {
            throw new ApplicationError("AGENT_RUN_NOT_FOUND", "Agent run was not found.");
        }

        return agentRun;
    }

    private static Approval GetPendingApproval(AgentRun agentRun)
    {
        var approval = agentRun.Approvals.SingleOrDefault(x => x.Status == ApprovalStatus.Pending);
        if (approval is null)
        {
            throw new ApplicationError("APPROVAL_NOT_PENDING", "No pending approval exists for this agent run.");
        }

        return approval;
    }

    private static void EnsureAwaitingApproval(AgentRun agentRun)
    {
        if (agentRun.CurrentStage != AgentStage.AwaitingApproval || agentRun.Status != AgentRunStatus.WaitingForApproval)
        {
            throw new ApplicationError("INVALID_WORKFLOW_TRANSITION", "Agent run is not waiting for approval.");
        }
    }
}
