using SmartAIAgent.Application.Common;
using SmartAIAgent.Application.Models.RequirementAnalysis;
using SmartAIAgent.Application.Models.Requirements;

namespace SmartAIAgent.Application.Interfaces;

public interface IRequirementService
{
    Task<RequirementDto> CreateAsync(CreateRequirementRequest request, CancellationToken cancellationToken);
    Task<PagedResult<RequirementSummaryDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<RequirementDetailsDto> GetByIdAsync(Guid requirementId, CancellationToken cancellationToken);
    Task<RequirementAnalysisDto> GetAnalysisAsync(Guid requirementId, CancellationToken cancellationToken);
}
