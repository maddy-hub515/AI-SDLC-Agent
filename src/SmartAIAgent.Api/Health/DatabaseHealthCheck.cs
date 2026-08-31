using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartAIAgent.Infrastructure.Persistence;

namespace SmartAIAgent.Api.Health;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly SmartAIAgentDbContext _dbContext;

    public DatabaseHealthCheck(SmartAIAgentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("Database is reachable.")
            : HealthCheckResult.Unhealthy("Database is not reachable.");
    }
}
