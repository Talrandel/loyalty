using LoyaltyApp.Db;
using LoyaltyApp.Models;
using LoyaltyApp.Services;
using LoyaltyApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyApp.Tests.Integration;

public class ActionEntryServiceTests
{
    private static DbContextOptions<ApplicationDbContext> GetOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllActions_WithUserAndCustomer()
    {
        // Arrange
        var options = GetOptions();
        var factory = new TestDbContextFactory(options);
        var service = new ActionEntryService(factory);
        await using var db = factory.CreateDbContext();

        var user = new User { UserName = "admin" };
        var customer = new Customer { Name = "Петя" };
        db.Users.Add(user);
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        db.ActionEntries.AddRange(
            new ActionEntry { ActionType = ActionType.CreateCustomer, UserId = user.Id, CustomerId = customer.Id, Date = DateTime.UtcNow },
            new ActionEntry { ActionType = ActionType.IncreaseBalance, UserId = user.Id, CustomerId = customer.Id, Amount = 50, Date = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        // Act
        var actions = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, actions.Count);
        Assert.All(actions, a => Assert.NotNull(a.User));
        Assert.All(actions, a => Assert.NotNull(a.Customer));
    }

    [Fact]
    public async Task GetByCustomerAsync_ReturnsOnlyThisCustomerActions()
    {
        // Arrange
        var options = GetOptions();
        var factory = new TestDbContextFactory(options);
        var service = new ActionEntryService(factory);
        await using var db = factory.CreateDbContext();

        var user = new User { UserName = "testuser" };
        var customer1 = new Customer { Name = "A" };
        var customer2 = new Customer { Name = "B" };
        db.Users.Add(user);
        db.Customers.AddRange(customer1, customer2);
        await db.SaveChangesAsync();

        db.ActionEntries.AddRange(
            new ActionEntry { ActionType = ActionType.CreateCustomer, UserId = user.Id, CustomerId = customer1.Id, Date = DateTime.UtcNow },
            new ActionEntry { ActionType = ActionType.EditCustomer, UserId = user.Id, CustomerId = customer2.Id, Date = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        // Act
        var actions = await service.GetByCustomerAsync(customer1.Id);

        // Assert
        Assert.Single(actions);
        Assert.Equal(ActionType.CreateCustomer, actions[0].ActionType);
        Assert.Equal(customer1.Id, actions[0].CustomerId);
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsOnlyThisUserActions()
    {
        // Arrange
        var options = GetOptions();
        var factory = new TestDbContextFactory(options);
        var service = new ActionEntryService(factory);
        await using var db = factory.CreateDbContext();

        var user1 = new User { UserName = "U1" };
        var user2 = new User { UserName = "U2" };
        var customer = new Customer { Name = "Клиент" };
        db.Users.AddRange(user1, user2);
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        db.ActionEntries.AddRange(
            new ActionEntry { ActionType = ActionType.CreateUser, UserId = user1.Id, Date = DateTime.UtcNow },
            new ActionEntry { ActionType = ActionType.IncreaseBalance, UserId = user2.Id, CustomerId = customer.Id, Date = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        // Act
        var user1Actions = await service.GetByUserAsync(user1.Id);
        var user2Actions = await service.GetByUserAsync(user2.Id);

        // Assert
        Assert.Single(user1Actions);
        Assert.Equal(ActionType.CreateUser, user1Actions[0].ActionType);
        Assert.Single(user2Actions);
        Assert.Equal(ActionType.IncreaseBalance, user2Actions[0].ActionType);
    }
}