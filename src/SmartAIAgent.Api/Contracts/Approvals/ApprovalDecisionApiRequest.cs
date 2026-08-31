using System.ComponentModel.DataAnnotations;

namespace SmartAIAgent.Api.Contracts.Approvals;

public sealed class ApprovalDecisionApiRequest
{
    [MaxLength(2000)]
    public string? Comment { get; init; }
}
