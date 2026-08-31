using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SmartAIAgent.Application.Interfaces;
using SmartAIAgent.Application.Options;
using SmartAIAgent.Infrastructure.Persistence;
using SmartAIAgent.Infrastructure.Services;

namespace SmartAIAgent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=SmartAIAgent;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));
        services.Configure<RequirementAnalysisOptions>(configuration.GetSection(RequirementAnalysisOptions.SectionName));
        services.AddDbContext<SmartAIAgentDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<SmartAIAgentDbContext>());
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IPromptService, FilePromptService>();
        services.AddHttpClient<ILlmService, OllamaLlmService>((provider, client) =>
        {
            var aiOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AiOptions>>().Value;
            client.BaseAddress = new Uri(aiOptions.BaseUrl, UriKind.Absolute);
        });

        return services;
    }

    public static void ConfigureSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
    }
}
