using System.ComponentModel.DataAnnotations;

namespace SmartAIAgent.Api.Contracts.Approvals;

public sealed class RejectionApiRequest
{
    [Required]
    [MaxLength(2000)]
    public string Reason { get; init; } = string.Empty;
}
