namespace SmartAIAgent.Application.Models.Requirements;

public sealed class CreateRequirementRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
