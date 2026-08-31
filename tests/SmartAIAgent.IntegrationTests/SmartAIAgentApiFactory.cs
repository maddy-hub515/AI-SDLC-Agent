using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartAIAgent.Application.Interfaces;
using SmartAIAgent.Application.Models.RequirementAnalysis;
using SmartAIAgent.Infrastructure.Persistence;

namespace SmartAIAgent.IntegrationTests;

public sealed class SmartAIAgentApiFactory : WebApplicationFactory<Program>
{
    private string? _connectionString;
    private readonly RequirementAnalysisResult _result = new()
    {
        UserStory = new RequirementAnalysisUserStoryResult
        {
            Title = "Remove Outcode Restriction from Linked Case Assignment",
            Description = "As a case management system, I want linked case assignment to consider eligible officers regardless of outcode so that valid officers are not excluded by the outcode restriction."
        },
        AcceptanceCriteria =
        [
            "Linked case assignment must not restrict eligible officers based on outcode.",
            "All other officer eligibility rules must remain unchanged.",
            "Existing assignment behavior outside the changed rule must remain unaffected."
        ],
        TechnicalAreas =
        [
            "Linked Case Assignment",
            "Officer Eligibility"
        ],
        DevelopmentTasks =
        [
            "Analyze the existing linked case assignment logic.",
            "Remove the outcode restriction.",
            "Add regression coverage for officers with different outcodes."
        ],
        Assumptions =
        [
            "Existing officer eligibility rules remain unchanged."
        ]
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var existingDescriptor = services.SingleOrDefault(x => x.ServiceType == typeof(DbContextOptions<SmartAIAgentDbContext>));
            if (existingDescriptor is not null)
            {
                services.Remove(existingDescriptor);
            }

            services.RemoveAll<ILlmService>();

            _connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database=SmartAIAgent_IntegrationTests_{Guid.NewGuid():N};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

            services.AddDbContext<SmartAIAgentDbContext>(options => options.UseSqlServer(_connectionString));
            services.AddScoped<ILlmService>(_ => new FakeIntegrationLlmService(_result));

        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_connectionString))
                {
                    var options = new DbContextOptionsBuilder<SmartAIAgentDbContext>()
                        .UseSqlServer(_connectionString)
                        .Options;

                    using var dbContext = new SmartAIAgentDbContext(options);
                    dbContext.Database.EnsureDeleted();
                }
            }
            catch
            {
            }
        }

        base.Dispose(disposing);
    }
}

internal sealed class FakeIntegrationLlmService : ILlmService
{
    private readonly RequirementAnalysisResult _result;

    public FakeIntegrationLlmService(RequirementAnalysisResult result)
    {
        _result = result;
    }

    public Task<T> GenerateStructuredAsync<T>(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        return Task.FromResult((T)(object)new RequirementAnalysisResult
        {
            UserStory = new RequirementAnalysisUserStoryResult
            {
                Title = _result.UserStory!.Title,
                Description = _result.UserStory.Description
            },
            AcceptanceCriteria = [.. _result.AcceptanceCriteria],
            TechnicalAreas = [.. _result.TechnicalAreas],
            DevelopmentTasks = [.. _result.DevelopmentTasks],
            Assumptions = [.. _result.Assumptions]
        });
    }
}
