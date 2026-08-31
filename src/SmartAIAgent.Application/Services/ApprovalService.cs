using Microsoft.EntityFrameworkCore;
using SmartAIAgent.Application.Interfaces;
using SmartAIAgent.Application.Models.Approvals;

namespace SmartAIAgent.Application.Services;

public sealed class ApprovalService : IApprovalService
{
    private readonly IApplicationDbContext _dbContext;

    public ApprovalService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ApprovalDto>> GetByAgentRunIdAsync(Guid agentRunId, CancellationToken cancellationToken)
    {
        return await _dbContext.Approvals
            .AsNoTracking()
            .Where(x => x.AgentRunId == agentRunId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new ApprovalDto
            {
                Id = x.Id,
                Type = x.Type,
                Status = x.Status,
                Comment = x.Comment,
                CreatedAtUtc = x.CreatedAtUtc,
                DecidedAtUtc = x.DecidedAtUtc
            })
            .ToArrayAsync(cancellationToken);
    }
}
