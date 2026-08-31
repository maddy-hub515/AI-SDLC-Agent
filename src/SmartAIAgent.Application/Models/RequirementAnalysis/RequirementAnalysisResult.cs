using System.ComponentModel.DataAnnotations;

namespace SmartAIAgent.Application.Models.RequirementAnalysis;

public sealed class RequirementAnalysisResult : IValidatableObject
{
    public RequirementAnalysisUserStoryResult? UserStory { get; init; }
    public List<string> AcceptanceCriteria { get; init; } = [];
    public List<string> TechnicalAreas { get; init; } = [];
    public List<string> DevelopmentTasks { get; init; } = [];
    public List<string> Assumptions { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (UserStory is null)
        {
            yield return new ValidationResult("userStory is required.", [nameof(UserStory)]);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(UserStory.Title))
            {
                yield return new ValidationResult("userStory.title is required.", [$"{nameof(UserStory)}.{nameof(UserStory.Title)}"]);
            }

            if (string.IsNullOrWhiteSpace(UserStory.Description))
            {
                yield return new ValidationResult("userStory.description is required.", [$"{nameof(UserStory)}.{nameof(UserStory.Description)}"]);
            }
        }

        if (AcceptanceCriteria.Count == 0 || AcceptanceCriteria.All(string.IsNullOrWhiteSpace))
        {
            yield return new ValidationResult("At least one acceptance criterion is required.", [nameof(AcceptanceCriteria)]);
        }
    }
}

public sealed class RequirementAnalysisUserStoryResult
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
