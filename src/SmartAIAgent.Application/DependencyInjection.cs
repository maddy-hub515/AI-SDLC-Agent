using Microsoft.Extensions.DependencyInjection;
using SmartAIAgent.Application.Interfaces;
using SmartAIAgent.Application.Services;

namespace SmartAIAgent.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRequirementService, RequirementService>();
        services.AddScoped<IWorkflowService, WorkflowService>();
        services.AddScoped<IApprovalService, ApprovalService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IRequirementAgent, RequirementAgent>();

        return services;
    }
}
