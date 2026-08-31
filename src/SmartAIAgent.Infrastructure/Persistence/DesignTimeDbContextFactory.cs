using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SmartAIAgent.Infrastructure.Persistence;

namespace SmartAIAgent.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SmartAIAgentDbContext>
{
    public SmartAIAgentDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<SmartAIAgentDbContext>();
        builder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SmartAIAgent;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
        return new SmartAIAgentDbContext(builder.Options);
    }
}
