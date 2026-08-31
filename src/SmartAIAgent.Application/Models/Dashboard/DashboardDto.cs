namespace SmartAIAgent.Application.Models.Dashboard;

public sealed class DashboardDto
{
    public int TotalRequirements { get; init; }
    public int ActiveAgentRuns { get; init; }
    public int PendingApprovals { get; init; }
    public int CompletedRuns { get; init; }
    public int FailedRuns { get; init; }
}
