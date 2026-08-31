using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartAIAgent.Application.Common;
using SmartAIAgent.Application.Models.Requirements;
using SmartAIAgent.Application.Services;
using SmartAIAgent.Domain.Enums;
using SmartAIAgent.UnitTests.TestHelpers;

namespace SmartAIAgent.UnitTests;

public sealed class RequirementServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateRequirement()
    {
        await using var handle = TestDbContextFactory.Create();
        var dbContext = handle.DbContext;
        var now = new DateTime(2026, 8, 28, 0, 0, 0, DateTimeKind.Utc);
        var service = new RequirementService(dbContext, new FakeDateTimeProvider(now), TestLoggerFactory.Create<RequirementService>());

        var result = await service.CreateAsync(new CreateRequirementRequest
        {
            Title = "Build dashboard",
            Description = "Create a requirement dashboard"
        }, CancellationToken.None);

        result.Title.Should().Be("Build dashboard");
        result.Status.Should().Be(RequirementStatus.Submitted);
        dbContext.Requirements.Should().ContainSingle();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnRequirement()
    {
        await using var handle = TestDbContextFactory.Create();
        var dbContext = handle.DbContext;
        var now = new DateTime(2026, 8, 28, 0, 0, 0, DateTimeKind.Utc);
        dbContext.Requirements.Add(new()
        {
            Id = Guid.NewGuid(),
            Title = "Requirement A",
            Description = "Description A",
            Status = RequirementStatus.Submitted,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });
        await dbContext.SaveChangesAsync();

        var service = new RequirementService(dbContext, new FakeDateTimeProvider(now), TestLoggerFactory.Create<RequirementService>());
        var requirement = await dbContext.Requirements.FirstAsync();

        var result = await service.GetByIdAsync(requirement.Id, CancellationToken.None);

        result.Title.Should().Be("Requirement A");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowWhenMissing()
    {
        await using var handle = TestDbContextFactory.Create();
        var dbContext = handle.DbContext;
        var service = new RequirementService(dbContext, new FakeDateTimeProvider(DateTime.UtcNow), TestLoggerFactory.Create<RequirementService>());

        var action = async () => await service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        var exception = await action.Should().ThrowAsync<ApplicationError>();
        exception.Which.Code.Should().Be("REQUIREMENT_NOT_FOUND");
    }
}
