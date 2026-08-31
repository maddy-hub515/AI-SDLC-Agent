using SmartAIAgent.Application.Models.Approvals;

namespace SmartAIAgent.Application.Interfaces;

public interface IApprovalService
{
    Task<IReadOnlyCollection<ApprovalDto>> GetByAgentRunIdAsync(Guid agentRunId, CancellationToken cancellationToken);
}
