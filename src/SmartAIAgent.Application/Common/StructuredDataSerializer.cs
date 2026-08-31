using System.Text.Json;

namespace SmartAIAgent.Application.Common;

internal static class StructuredDataSerializer
{
    public static string SerializeCollection(IEnumerable<string> values)
    {
        return JsonSerializer.Serialize(values.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray());
    }

    public static IReadOnlyCollection<string> DeserializeCollection(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<string>();
        }

        return JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
    }
}
