namespace SmartAIAgent.Domain.Enums;

public enum AgentStage
{
    None = 0,
    RequirementAnalysis = 1,
    UserStoryGeneration = 2,
    AiProcessing = 3,
    UserStoryPersisted = 4,
    AwaitingApproval = 5,
    Development = 6,
    Testing = 7,
    CodeReview = 8,
    Deployment = 9,
    Completed = 10
}
