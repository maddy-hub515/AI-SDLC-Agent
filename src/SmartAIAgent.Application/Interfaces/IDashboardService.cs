using SmartAIAgent.Application.Models.Dashboard;

namespace SmartAIAgent.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetAsync(CancellationToken cancellationToken);
}
