namespace SmartAIAgent.Application.Options;

public sealed class RequirementAnalysisOptions
{
    public const string SectionName = "RequirementAnalysis";

    public int MaxAutomaticRetries { get; init; } = 10;
    public int TitleMaxLength { get; init; } = 200;
    public int DescriptionMaxLength { get; init; } = 4000;
    public int ListItemMaxLength { get; init; } = 500;
    public int MaxListItems { get; init; } = 20;
    public int ErrorMessageMaxLength { get; init; } = 500;
}
