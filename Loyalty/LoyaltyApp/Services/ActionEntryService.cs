using LoyaltyApp.Db;
using LoyaltyApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyApp.Services;

public sealed class ActionEntryService : IActionEntryService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public ActionEntryService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<ActionEntry>> GetAllAsync()
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        return await dbContext.ActionEntries
            .Include(a => a.User)
            .Include(a => a.Customer)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<List<ActionEntry>> GetByCustomerAsync(int customerId)
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        return await dbContext.ActionEntries
            .Where(a => a.CustomerId == customerId)
            .Include(a => a.User)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<List<ActionEntry>> GetByUserAsync(int userId)
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        return await dbContext.ActionEntries
            .Where(a => a.UserId == userId)
            .Include(a => a.Customer)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }
}