using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartAIAgent.Application.Common;
using SmartAIAgent.Application.Interfaces;
using SmartAIAgent.Application.Models.AgentRuns;
using SmartAIAgent.Application.Models.RequirementAnalysis;
using SmartAIAgent.Application.Models.Requirements;
using SmartAIAgent.Application.Models.UserStories;
using SmartAIAgent.Domain.Entities;
using SmartAIAgent.Domain.Enums;

namespace SmartAIAgent.Application.Services;

public sealed class RequirementService : IRequirementService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<RequirementService> _logger;

    public RequirementService(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider, ILogger<RequirementService> logger)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<RequirementDto> CreateAsync(CreateRequirementRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ApplicationError("VALIDATION_ERROR", "Requirement title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new ApplicationError("VALIDATION_ERROR", "Requirement description is required.");
        }

        var now = _dateTimeProvider.UtcNow;
        var requirement = new Requirement
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Status = RequirementStatus.Submitted,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _dbContext.Requirements.Add(requirement);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Requirement {RequirementId} created with status {Status}", requirement.Id, requirement.Status);

        return MapRequirement(requirement);
    }

    public async Task<PagedResult<RequirementSummaryDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var items = await _dbContext.Requirements
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new RequirementSummaryDto
            {
                Id = x.Id,
                Title = x.Title,
                Status = x.Status,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<RequirementSummaryDto>
        {
            Items = items,
            TotalCount = items.Count
        };
    }

    public async Task<RequirementDetailsDto> GetByIdAsync(Guid requirementId, CancellationToken cancellationToken)
    {
        var requirement = await _dbContext.Requirements
            .AsNoTracking()
            .Include(x => x.UserStories)
                .ThenInclude(x => x.AcceptanceCriteria)
            .Include(x => x.AgentRuns)
            .FirstOrDefaultAsync(x => x.Id == requirementId, cancellationToken);

        if (requirement is null)
        {
            throw new ApplicationError("REQUIREMENT_NOT_FOUND", "Requirement was not found.");
        }

        return new RequirementDetailsDto
        {
            Id = requirement.Id,
            Title = requirement.Title,
            Description = requirement.Description,
            Status = requirement.Status,
            CreatedAtUtc = requirement.CreatedAtUtc,
            UpdatedAtUtc = requirement.UpdatedAtUtc,
            UserStories = requirement.UserStories
                .OrderBy(x => x.CreatedAtUtc)
                .Select(x => new UserStoryDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    AcceptanceCriteria = x.AcceptanceCriteria.Select(c => c.Value).ToArray(),
                    TechnicalAreas = StructuredDataSerializer.DeserializeCollection(x.TechnicalAreasJson),
                    DevelopmentTasks = StructuredDataSerializer.DeserializeCollection(x.DevelopmentTasksJson),
                    Assumptions = StructuredDataSerializer.DeserializeCollection(x.AssumptionsJson),
                    CreatedAtUtc = x.CreatedAtUtc
                })
                .ToArray(),
            AgentRuns = requirement.AgentRuns
                .OrderByDescending(x => x.StartedAtUtc)
                .Select(x => new AgentRunSummaryDto
                {
                    Id = x.Id,
                    Status = x.Status,
                    CurrentStage = x.CurrentStage,
                    Provider = x.Provider,
                    Model = x.Model,
                    PromptVersion = x.PromptVersion,
                    RetryCount = x.RetryCount,
                    StartedAtUtc = x.StartedAtUtc,
                    CompletedAtUtc = x.CompletedAtUtc
                })
                .ToArray()
        };
    }

    public async Task<RequirementAnalysisDto> GetAnalysisAsync(Guid requirementId, CancellationToken cancellationToken)
    {
        var requirement = await _dbContext.Requirements
            .AsNoTracking()
            .Include(x => x.UserStories)
                .ThenInclude(x => x.AcceptanceCriteria)
            .Include(x => x.AgentRuns)
                .ThenInclude(x => x.Approvals)
            .Include(x => x.AgentRuns)
                .ThenInclude(x => x.WorkflowEvents)
            .FirstOrDefaultAsync(x => x.Id == requirementId, cancellationToken);

        if (requirement is null)
        {
            throw new ApplicationError("REQUIREMENT_NOT_FOUND", "Requirement was not found.");
        }

        var latestRun = requirement.AgentRuns
            .OrderByDescending(x => x.StartedAtUtc)
            .FirstOrDefault();

        var latestStory = requirement.UserStories
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefault();

        return new RequirementAnalysisDto
        {
            RequirementId = requirement.Id,
            LatestRun = latestRun is null ? null : AgentRunMapper.MapDetails(latestRun),
            Analysis = latestStory is null
                ? null
                : new RequirementAnalysisResultDto
                {
                    UserStoryId = latestStory.Id,
                    Title = latestStory.Title,
                    Description = latestStory.Description,
                    AcceptanceCriteria = latestStory.AcceptanceCriteria.Select(x => x.Value).ToArray(),
                    TechnicalAreas = StructuredDataSerializer.DeserializeCollection(latestStory.TechnicalAreasJson),
                    DevelopmentTasks = StructuredDataSerializer.DeserializeCollection(latestStory.DevelopmentTasksJson),
                    Assumptions = StructuredDataSerializer.DeserializeCollection(latestStory.AssumptionsJson),
                    CreatedAtUtc = latestStory.CreatedAtUtc
                }
        };
    }

    private static RequirementDto MapRequirement(Requirement requirement) => new()
    {
        Id = requirement.Id,
        Title = requirement.Title,
        Description = requirement.Description,
        Status = requirement.Status,
        CreatedAtUtc = requirement.CreatedAtUtc,
        UpdatedAtUtc = requirement.UpdatedAtUtc
    };
}
