using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace SmartAIAgent.IntegrationTests;

public sealed class ApiTests : IClassFixture<SmartAIAgentApiFactory>
{
    private readonly HttpClient _client;

    public ApiTests(SmartAIAgentApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostRequirements_ShouldCreateRequirement()
    {
        var response = await _client.PostAsJsonAsync("/api/requirements", new { title = "Requirement A", description = "Description A" });

        if (response.StatusCode != HttpStatusCode.Created)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Unexpected status {(int)response.StatusCode}: {body}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task RequirementLifecycleEndpoints_ShouldWork()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/requirements", new { title = "Requirement B", description = "Description B" });
        var created = await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<RequirementPayload>>();

        var getRequirementResponse = await _client.GetAsync($"/api/requirements/{created!.Data!.Id}");
        getRequirementResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var runResponse = await _client.PostAsync($"/api/requirements/{created.Data.Id}/analyze", null);
        runResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var runPayload = await runResponse.Content.ReadFromJsonAsync<ApiEnvelope<AgentRunPayload>>();

        var getRunResponse = await _client.GetAsync($"/api/agent-runs/{runPayload!.Data!.Id}");
        getRunResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var analysisResponse = await _client.GetAsync($"/api/requirements/{created.Data.Id}/analysis");
        analysisResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var approveResponse = await _client.PostAsJsonAsync($"/api/agent-runs/{runPayload.Data.Id}/approve", new { comment = "Approved" });
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RejectEndpoint_ShouldWork()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/requirements", new { title = "Requirement C", description = "Description C" });
        var created = await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<RequirementPayload>>();
        var runResponse = await _client.PostAsync($"/api/requirements/{created!.Data!.Id}/analyze", null);
        var runPayload = await runResponse.Content.ReadFromJsonAsync<ApiEnvelope<AgentRunPayload>>();

        var rejectResponse = await _client.PostAsJsonAsync($"/api/agent-runs/{runPayload!.Data!.Id}/reject", new { reason = "Needs revisions" });

        rejectResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DuplicateAnalyze_ShouldReturnConflict()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/requirements", new { title = "Requirement D", description = "Description D" });
        var created = await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<RequirementPayload>>();

        var firstResponse = await _client.PostAsync($"/api/requirements/{created!.Data!.Id}/analyze", null);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondResponse = await _client.PostAsync($"/api/requirements/{created.Data.Id}/analyze", null);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnSuccess()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private sealed class ApiEnvelope<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
    }

    private sealed class RequirementPayload
    {
        public Guid Id { get; set; }
    }

    private sealed class AgentRunPayload
    {
        public Guid Id { get; set; }
    }
}
