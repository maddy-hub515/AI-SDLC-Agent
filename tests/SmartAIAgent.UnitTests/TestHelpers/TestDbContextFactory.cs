using Microsoft.EntityFrameworkCore;
using SmartAIAgent.Infrastructure.Persistence;

namespace SmartAIAgent.UnitTests.TestHelpers;

internal static class TestDbContextFactory
{
    public static TestDbContextHandle Create()
    {
        var options = new DbContextOptionsBuilder<SmartAIAgentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new SmartAIAgentDbContext(options);
        dbContext.Database.EnsureCreated();

        return new TestDbContextHandle(dbContext);
    }
}

internal sealed class TestDbContextHandle : IAsyncDisposable
{
    public TestDbContextHandle(SmartAIAgentDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public SmartAIAgentDbContext DbContext { get; }

    public async ValueTask DisposeAsync()
    {
        await DbContext.DisposeAsync();
    }
}
