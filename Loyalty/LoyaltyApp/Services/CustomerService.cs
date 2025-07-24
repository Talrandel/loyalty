using LoyaltyApp.Db;
using LoyaltyApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyApp.Services;

internal sealed class CustomerService : ICustomerService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    
    public CustomerService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }
    
    public async Task CreateAsync(int actingUserId, Customer customer)
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        customer.CreatedAt = DateTime.UtcNow;
        dbContext.Customers.Add(customer);
        
        await dbContext.SaveChangesAsync();

        dbContext.ActionEntries.Add(new ActionEntry
        {
            ActionType = ActionType.CreateCustomer,
            UserId = actingUserId,
            CustomerId = customer.Id,
            Date = DateTime.UtcNow,
            Description = "Клиент зарегистрирован"
        });
        
        await dbContext.SaveChangesAsync();
    }

    public async Task IncreaseBalance(int actingUserId, Customer customer, decimal amount)
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        var entity = await dbContext.Customers.FindAsync(customer.Id);
        if (entity != null)
        {
            entity.Balance += amount;
        
            dbContext.ActionEntries.Add(new ActionEntry
            {
                ActionType = ActionType.IncreaseBalance,
                UserId = actingUserId,
                CustomerId = entity.Id,
                Amount = amount,
                Date = DateTime.UtcNow,
                Description = $"Начислено {amount} руб."
            });
            
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task WithdrawBalance(int actingUserId, Customer customer, decimal amount)
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        var entity = await dbContext.Customers.FindAsync(customer.Id);
        if (entity != null && entity.Balance >= amount)
        {
            entity.Balance -= amount;
            
            dbContext.ActionEntries.Add(new ActionEntry
            {
                ActionType = ActionType.WithdrawBalance,
                UserId = actingUserId,
                CustomerId = entity.Id,
                Amount = amount,
                Date = DateTime.UtcNow,
                Description = $"Списано {amount} руб."
            });
            
            await dbContext.SaveChangesAsync();
        }
    }
    public async Task EditAsync(int actingUserId, Customer customer)
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        var entity = await dbContext.Customers.FindAsync(customer.Id);
        if (entity != null)
        {
            entity.Name = customer.Name;
            entity.PhoneLastFourDigits = customer.PhoneLastFourDigits;

            dbContext.ActionEntries.Add(new ActionEntry
            {
                ActionType = ActionType.EditCustomer,
                UserId = actingUserId,
                CustomerId = entity.Id,
                Date = DateTime.UtcNow,
                Description = $"Клиент изменён"
            });
            
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<Customer> GetCustomer(int id)
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        return await dbContext.Customers.FindAsync(id) ?? throw new NullReferenceException("Клиент не найден");
    }

    public async Task<List<Customer>> GetCustomers()
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        return await dbContext.Customers
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }
}