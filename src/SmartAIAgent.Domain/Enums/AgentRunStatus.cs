namespace SmartAIAgent.Domain.Enums;

public enum AgentRunStatus
{
    Created = 0,
    Running = 1,
    WaitingForApproval = 2,
    Approved = 3,
    Rejected = 4,
    Completed = 5,
    Failed = 6
}
