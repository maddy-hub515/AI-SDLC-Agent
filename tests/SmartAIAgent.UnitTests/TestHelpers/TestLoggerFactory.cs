using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SmartAIAgent.UnitTests.TestHelpers;

internal static class TestLoggerFactory
{
    public static ILogger<T> Create<T>() => NullLogger<T>.Instance;
}
