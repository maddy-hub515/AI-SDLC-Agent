using SmartAIAgent.Domain.Enums;

namespace SmartAIAgent.Application.Models.AgentRuns;

public sealed class AgentRunSummaryDto
{
    public Guid Id { get; init; }
    public AgentRunStatus Status { get; init; }
    public AgentStage CurrentStage { get; init; }
    public string? Provider { get; init; }
    public string? Model { get; init; }
    public string? PromptVersion { get; init; }
    public int RetryCount { get; init; }
    public DateTime StartedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
}
