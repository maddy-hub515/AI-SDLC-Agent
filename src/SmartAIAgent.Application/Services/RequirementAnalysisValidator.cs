using SmartAIAgent.Application.Common;
using SmartAIAgent.Application.Models.RequirementAnalysis;
using SmartAIAgent.Application.Options;

namespace SmartAIAgent.Application.Services;

internal static class RequirementAnalysisValidator
{
    public static void ValidateAndNormalize(RequirementAnalysisResult result, RequirementAnalysisOptions options)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(options);

        if (result.UserStory is null)
        {
            throw new LlmException("AI analysis did not include a user story.");
        }

        ValidateRequiredString(result.UserStory.Title, options.TitleMaxLength, "AI analysis user story title");
        ValidateRequiredString(result.UserStory.Description, options.DescriptionMaxLength, "AI analysis user story description");

        var acceptanceCriteria = NormalizeItems(result.AcceptanceCriteria, options, true, "acceptance criteria");
        var technicalAreas = NormalizeItems(result.TechnicalAreas, options, false, "technical areas");
        var developmentTasks = NormalizeItems(result.DevelopmentTasks, options, true, "development tasks");
        var assumptions = NormalizeItems(result.Assumptions, options, false, "assumptions");

        result.AcceptanceCriteria.Clear();
        result.AcceptanceCriteria.AddRange(acceptanceCriteria);
        result.TechnicalAreas.Clear();
        result.TechnicalAreas.AddRange(technicalAreas);
        result.DevelopmentTasks.Clear();
        result.DevelopmentTasks.AddRange(developmentTasks);
        result.Assumptions.Clear();
        result.Assumptions.AddRange(assumptions);
    }

    private static List<string> NormalizeItems(IReadOnlyCollection<string> items, RequirementAnalysisOptions options, bool required, string name)
    {
        var normalized = items
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalized.Count > options.MaxListItems)
        {
            throw new LlmException($"AI analysis {name} exceeds the maximum allowed item count.");
        }

        if (required && normalized.Count == 0)
        {
            throw new LlmException($"AI analysis must include at least one {name.TrimEnd('s')}.");
        }

        foreach (var item in normalized)
        {
            ValidateRequiredString(item, options.ListItemMaxLength, $"AI analysis {name.TrimEnd('s')}");
        }

        return normalized;
    }

    private static void ValidateRequiredString(string? value, int maxLength, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new LlmException($"{name} is required.");
        }

        if (value.Trim().Length > maxLength)
        {
            throw new LlmException($"{name} exceeds the maximum allowed length.");
        }
    }
}
