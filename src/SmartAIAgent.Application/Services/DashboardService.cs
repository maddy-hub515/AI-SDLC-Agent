using Microsoft.EntityFrameworkCore;
using SmartAIAgent.Application.Interfaces;
using SmartAIAgent.Application.Models.Dashboard;
using SmartAIAgent.Domain.Enums;

namespace SmartAIAgent.Application.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _dbContext;

    public DashboardService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardDto> GetAsync(CancellationToken cancellationToken)
    {
        return new DashboardDto
        {
            TotalRequirements = await _dbContext.Requirements.CountAsync(cancellationToken),
            ActiveAgentRuns = await _dbContext.AgentRuns.CountAsync(x => x.Status == AgentRunStatus.Running || x.Status == AgentRunStatus.Created || x.Status == AgentRunStatus.WaitingForApproval, cancellationToken),
            PendingApprovals = await _dbContext.Approvals.CountAsync(x => x.Status == ApprovalStatus.Pending, cancellationToken),
            CompletedRuns = await _dbContext.AgentRuns.CountAsync(x => x.Status == AgentRunStatus.Completed || x.Status == AgentRunStatus.Approved, cancellationToken),
            FailedRuns = await _dbContext.AgentRuns.CountAsync(x => x.Status == AgentRunStatus.Failed || x.Status == AgentRunStatus.Rejected, cancellationToken)
        };
    }
}
