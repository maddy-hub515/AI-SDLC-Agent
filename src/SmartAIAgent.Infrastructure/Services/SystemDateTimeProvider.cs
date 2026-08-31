using SmartAIAgent.Application.Interfaces;

namespace SmartAIAgent.Infrastructure.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
