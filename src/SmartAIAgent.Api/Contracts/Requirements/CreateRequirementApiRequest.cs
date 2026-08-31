using System.ComponentModel.DataAnnotations;

namespace SmartAIAgent.Api.Contracts.Requirements;

public sealed class CreateRequirementApiRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string Description { get; init; } = string.Empty;
}
