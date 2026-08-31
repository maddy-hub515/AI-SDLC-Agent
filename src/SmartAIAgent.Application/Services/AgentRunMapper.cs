using SmartAIAgent.Application.Models.AgentRuns;
using SmartAIAgent.Application.Models.Approvals;
using SmartAIAgent.Application.Models.Workflow;
using SmartAIAgent.Domain.Entities;

namespace SmartAIAgent.Application.Services;

internal static class AgentRunMapper
{
    public static AgentRunDetailsDto MapDetails(AgentRun agentRun)
    {
        return new AgentRunDetailsDto
        {
            Id = agentRun.Id,
            RequirementId = agentRun.RequirementId,
            Status = agentRun.Status,
            CurrentStage = agentRun.CurrentStage,
            Provider = agentRun.Provider,
            Model = agentRun.Model,
            PromptVersion = agentRun.PromptVersion,
            RetryCount = agentRun.RetryCount,
            StartedAtUtc = agentRun.StartedAtUtc,
            CompletedAtUtc = agentRun.CompletedAtUtc,
            ErrorMessage = agentRun.ErrorMessage,
            Approvals = agentRun.Approvals
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => new ApprovalDto
                {
                    Id = x.Id,
                    Type = x.Type,
                    Status = x.Status,
                    Comment = x.Comment,
                    CreatedAtUtc = x.CreatedAtUtc,
                    DecidedAtUtc = x.DecidedAtUtc
                })
                .ToArray(),
            WorkflowEvents = agentRun.WorkflowEvents
                .OrderBy(x => x.CreatedAtUtc)
                .Select(x => new WorkflowEventDto
                {
                    Id = x.Id,
                    FromStage = x.FromStage,
                    ToStage = x.ToStage,
                    EventType = x.EventType,
                    Message = x.Message,
                    CreatedAtUtc = x.CreatedAtUtc
                })
                .ToArray()
        };
    }
}
