using Microsoft.AspNetCore.Mvc;
using SmartAIAgent.Api.Contracts.Approvals;
using SmartAIAgent.Application.Common;
using SmartAIAgent.Application.Interfaces;
using SmartAIAgent.Application.Models.AgentRuns;

namespace SmartAIAgent.Api.Controllers;

[ApiController]
[Route("api/agent-runs")]
public sealed class AgentRunsController : ControllerBase
{
    private readonly IWorkflowService _workflowService;

    public AgentRunsController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AgentRunDetailsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AgentRunDetailsDto>>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _workflowService.GetRunAsync(id, cancellationToken);
        return Ok(ApiResponse<AgentRunDetailsDto>.Ok(result));
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<AgentRunDetailsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AgentRunDetailsDto>>> ApproveAsync(Guid id, ApprovalDecisionApiRequest request, CancellationToken cancellationToken)
    {
        var result = await _workflowService.ApproveAsync(id, request.Comment, cancellationToken);
        return Ok(ApiResponse<AgentRunDetailsDto>.Ok(result));
    }

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(ApiResponse<AgentRunDetailsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AgentRunDetailsDto>>> RejectAsync(Guid id, RejectionApiRequest request, CancellationToken cancellationToken)
    {
        var result = await _workflowService.RejectAsync(id, request.Reason, cancellationToken);
        return Ok(ApiResponse<AgentRunDetailsDto>.Ok(result));
    }
}
