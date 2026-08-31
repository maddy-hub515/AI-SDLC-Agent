using SmartAIAgent.Application.Models.AgentRuns;

namespace SmartAIAgent.Application.Interfaces;

public interface IWorkflowService
{
    Task<AgentRunDetailsDto> StartAsync(Guid requirementId, CancellationToken cancellationToken);
    Task<AgentRunDetailsDto> ApproveAsync(Guid agentRunId, string? comment, CancellationToken cancellationToken);
    Task<AgentRunDetailsDto> RejectAsync(Guid agentRunId, string reason, CancellationToken cancellationToken);
    Task<AgentRunDetailsDto> GetRunAsync(Guid agentRunId, CancellationToken cancellationToken);
}
