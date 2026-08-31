using SmartAIAgent.Application.Interfaces;

namespace SmartAIAgent.UnitTests.TestHelpers;

internal sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public FakeDateTimeProvider(DateTime utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTime UtcNow { get; set; }
}
