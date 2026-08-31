using SmartAIAgent.Application.Interfaces;
using SmartAIAgent.Application.Models.AgentRuns;

namespace SmartAIAgent.UnitTests.TestHelpers;

internal sealed class FakeRequirementAgent : IRequirementAgent
{
    private readonly Func<Guid, CancellationToken, Task<AgentRunDetailsDto>> _handler;

    public FakeRequirementAgent(Func<Guid, CancellationToken, Task<AgentRunDetailsDto>>? handler = null)
    {
        _handler = handler ?? ((_, _) => throw new NotSupportedException("StartAsync is not used in this test."));
    }

    public Task<AgentRunDetailsDto> AnalyzeAsync(Guid requirementId, CancellationToken cancellationToken)
    {
        return _handler(requirementId, cancellationToken);
    }
}
