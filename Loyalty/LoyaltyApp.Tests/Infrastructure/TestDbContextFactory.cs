using LoyaltyApp.Db;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyApp.Tests.Infrastructure;

internal sealed class TestDbContextFactory : IDbContextFactory<ApplicationDbContext>
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    internal TestDbContextFactory(DbContextOptions<ApplicationDbContext> options)
    {
        _options = options;
    }

    public ApplicationDbContext CreateDbContext()
    {
        return new ApplicationDbContext(_options);
    }
}