using LoyaltyApp.Db;
using LoyaltyApp.Models;
using LoyaltyApp.Services;
using LoyaltyApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyApp.Tests.Integration;

public class CustomerServiceIntegrationTests
{
    private static DbContextOptions<ApplicationDbContext> GetOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task CreateAsync_SavesCustomer_AndAddsActionEntry()
    {
        // Arrange
        var options = GetOptions();
        var factory = new TestDbContextFactory(options);
        var service = new CustomerService(factory);
        await using var db = factory.CreateDbContext();

        var customer = new Customer { Name = "Интеграция", PhoneLastFourDigits = "5678" };

        await service.CreateAsync(1, customer);

        var fromDb = await db.Customers.FirstOrDefaultAsync(c => c.Name == "Интеграция");
        Assert.NotNull(fromDb);
        Assert.True(fromDb.CreatedAt > DateTime.MinValue);

        var entry = await db.ActionEntries.FirstOrDefaultAsync(a => a.CustomerId == fromDb.Id && a.ActionType == ActionType.CreateCustomer);
        Assert.NotNull(entry);
    }

    [Fact]
    public async Task IncreaseBalance_UpdatesBalance_AndAddsActionEntry()
    {
        // Arrange
        var options = GetOptions();
        var factory = new TestDbContextFactory(options);
        var service = new CustomerService(factory);
        await using var db = factory.CreateDbContext();

        var customer = new Customer { Name = "Test", PhoneLastFourDigits = "1234", Balance = 100 };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        await service.IncreaseBalance(1, customer, 200);

        await using var dbNew = factory.CreateDbContext();
        var updated = await dbNew.Customers.FirstOrDefaultAsync(c => c.Id == customer.Id);
        Assert.Equal(300, updated.Balance);

        var entry = await dbNew.ActionEntries.FirstOrDefaultAsync(a => a.CustomerId == customer.Id && a.ActionType == ActionType.IncreaseBalance);
        Assert.NotNull(entry);
        Assert.Equal(200, entry.Amount);
    }

    [Fact]
    public async Task WithdrawBalance_DecreasesBalance_WhenEnoughMoney_AndAddsActionEntry()
    {
        // Arrange
        var options = GetOptions();
        var factory = new TestDbContextFactory(options);
        var service = new CustomerService(factory);
        await using var db = factory.CreateDbContext();

        var customer = new Customer { Name = "Test", PhoneLastFourDigits = "5678", Balance = 200 };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        await service.WithdrawBalance(1, customer, 50);

        await using var dbNew = factory.CreateDbContext();
        var updated = await dbNew.Customers.FirstOrDefaultAsync(c => c.Id == customer.Id);
        Assert.Equal(150, updated.Balance);

        // Если добавлена запись ActionEntry при списании, проверь:
        var entry = await dbNew.ActionEntries
            .FirstOrDefaultAsync(a => a.CustomerId == customer.Id 
                                      && a.ActionType == ActionType.WithdrawBalance);
        Assert.NotNull(entry);
        Assert.Equal(50, entry.Amount);
    }

    [Fact]
    public async Task EditAsync_UpdatesCustomer_AndAddsActionEntry()
    {
        // Arrange
        var options = GetOptions();
        var factory = new TestDbContextFactory(options);
        var service = new CustomerService(factory);
        await using var db = factory.CreateDbContext();

        var customer = new Customer { Name = "Old", PhoneLastFourDigits = "0000" };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var update = new Customer { Id = customer.Id, Name = "New", PhoneLastFourDigits = "9999" };
        await service.EditAsync(1, update);

        await using var dbNew = factory.CreateDbContext();
        var edited = await dbNew.Customers.FirstOrDefaultAsync(c => c.Id == customer.Id);
        Assert.Equal("New", edited.Name);
        Assert.Equal("9999", edited.PhoneLastFourDigits);

        var entry = await dbNew.ActionEntries.FirstOrDefaultAsync(a => a.CustomerId == customer.Id && a.ActionType == ActionType.EditCustomer);
        Assert.NotNull(entry);
    }

    [Fact]
    public async Task GetCustomer_ReturnsCorrectCustomer()
    {
        // Arrange
        var options = GetOptions();
        var factory = new TestDbContextFactory(options);
        var service = new CustomerService(factory);
        await using var db = factory.CreateDbContext();

        var customer = new Customer { Name = "Unique", PhoneLastFourDigits = "1234" };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var fromService = await service.GetCustomer(customer.Id);

        Assert.NotNull(fromService);
        Assert.Equal("Unique", fromService.Name);
    }

    [Fact]
    public async Task GetCustomers_ReturnsAllCustomers()
    {
        // Arrange
        var options = GetOptions();
        var factory = new TestDbContextFactory(options);
        var service = new CustomerService(factory);
        await using var db = factory.CreateDbContext();

        db.Customers.AddRange(
            new Customer { Name = "C1", PhoneLastFourDigits = "0001" },
            new Customer { Name = "C2", PhoneLastFourDigits = "0002" }
        );
        await db.SaveChangesAsync();

        var all = await service.GetCustomers();

        Assert.Equal(2, all.Count);
        Assert.Contains(all, c => c.Name == "C1");
        Assert.Contains(all, c => c.Name == "C2");
    }
}