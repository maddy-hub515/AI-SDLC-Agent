using Microsoft.AspNetCore.Mvc;
using SmartAIAgent.Api.Contracts.Requirements;
using SmartAIAgent.Application.Common;
using SmartAIAgent.Application.Interfaces;
using SmartAIAgent.Application.Models.AgentRuns;
using SmartAIAgent.Application.Models.RequirementAnalysis;
using SmartAIAgent.Application.Models.Requirements;

namespace SmartAIAgent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RequirementsController : ControllerBase
{
    private readonly IRequirementService _requirementService;
    private readonly IWorkflowService _workflowService;

    public RequirementsController(IRequirementService requirementService, IWorkflowService workflowService)
    {
        _requirementService = requirementService;
        _workflowService = workflowService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RequirementDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<RequirementDto>>> CreateAsync(CreateRequirementApiRequest request, CancellationToken cancellationToken)
    {
        var result = await _requirementService.CreateAsync(new CreateRequirementRequest
        {
            Title = request.Title,
            Description = request.Description
        }, cancellationToken);

        return CreatedAtRoute("GetRequirementById", new { id = result.Id }, ApiResponse<RequirementDto>.Ok(result));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetAsync(CancellationToken cancellationToken)
    {
        var result = await _requirementService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}", Name = "GetRequirementById")]
    [ProducesResponseType(typeof(ApiResponse<RequirementDetailsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RequirementDetailsDto>>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _requirementService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<RequirementDetailsDto>.Ok(result));
    }

    [HttpGet("{id:guid}/analysis")]
    [ProducesResponseType(typeof(ApiResponse<RequirementAnalysisDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RequirementAnalysisDto>>> GetAnalysisAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _requirementService.GetAnalysisAsync(id, cancellationToken);
        return Ok(ApiResponse<RequirementAnalysisDto>.Ok(result));
    }

    [HttpPost("{id:guid}/analyze")]
    [ProducesResponseType(typeof(ApiResponse<AgentRunDetailsDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<AgentRunDetailsDto>>> AnalyzeAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _workflowService.StartAsync(id, cancellationToken);
        return Created($"/api/agent-runs/{result.Id}", ApiResponse<AgentRunDetailsDto>.Ok(result));
    }

    [HttpPost("{id:guid}/runs")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<object>>> StartRunAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _workflowService.StartAsync(id, cancellationToken);
        return Created($"/api/agent-runs/{result.Id}", ApiResponse<object>.Ok(result));
    }
}
