using LoyaltyApp.Db;
using LoyaltyApp.Models;
using LoyaltyApp.Services;
using LoyaltyApp.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyApp.Tests.Integration;

public class UserServiceTests
{
    private static (DbContextOptions<ApplicationDbContext> options, IPasswordHasher<User> hasher) GetOptionsAndHasher()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        var hasher = new PasswordHasher<User>();
        return (options, hasher);
    }

    [Fact]
    public async Task CreateAsync_CreatesUserWithHashedPassword()
    {
        // Arrange
        var (options, hasher) = GetOptionsAndHasher();
        var factory = new TestDbContextFactory(options);
        var service = new UserService(hasher, factory);

        // Act
        _ = await service.CreateAsync(actingUserId: 1, login: "test", userName: "Test", password: "pass123");

        await using var db = factory.CreateDbContext();
        // Assert
        var fromDb = db.Users.FirstOrDefault();
        Assert.NotNull(fromDb);
        Assert.Equal("test", fromDb.Login);
        Assert.NotNull(fromDb.PasswordHash);
        Assert.NotEqual("pass123", fromDb.PasswordHash); // должен быть хеш, не сырой пароль
    }

    [Fact]
    public void Validate_ReturnsUser_IfPasswordCorrect()
    {
        // Arrange
        var (options, hasher) = GetOptionsAndHasher();
        var password = "Secret123";
        var user = new User { Login = "admin", UserName = "Admin" };
        user.PasswordHash = hasher.HashPassword(user, password);

        var factory = new TestDbContextFactory(options);
        var service = new UserService(hasher, factory);
        using var db = factory.CreateDbContext();
        
        db.Users.Add(user);
        db.SaveChanges();

        // Act
        var result = service.Validate("admin", "Secret123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("admin", result.Login);
    }

    [Fact]
    public void Validate_ReturnsNull_IfPasswordIncorrect()
    {
        // Arrange
        var (options, hasher) = GetOptionsAndHasher();
        var user = new User { Login = "admin", UserName = "Admin" };
        user.PasswordHash = hasher.HashPassword(user, "Secret123");

        var factory = new TestDbContextFactory(options);
        var service = new UserService(hasher, factory);
        using var db = factory.CreateDbContext();
        db.Users.Add(user);
        db.SaveChanges();

        // Act
        var result = service.Validate("admin", "WrongPassword");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserAsync_ChangesUserRole()
    {
        // Arrange
        var (options, hasher) = GetOptionsAndHasher();
        var user = new User { Login = "u", UserName = "User", Role = Role.Employee };

        var factory = new TestDbContextFactory(options);
        var service = new UserService(hasher, factory);
        await using var db = factory.CreateDbContext();
        
        db.Users.Add(user);
        await db.SaveChangesAsync();

        user.Role = Role.Admin;

        // Act
        await service.UpdateUserAsync(1, user);

        // Assert
        var fromDb = db.Users.First();
        Assert.Equal(Role.Admin, fromDb.Role);
    }

    [Fact]
    public async Task GetUsersAsync_ReturnsUsersExceptAdmin()
    {
        // Arrange
        var (options, hasher) = GetOptionsAndHasher();

        var factory = new TestDbContextFactory(options);
        var service = new UserService(hasher, factory);
        await using var db = factory.CreateDbContext();
        
        db.Users.AddRange(
            new User { Login = "admin", UserName = "A", Role = Role.Admin },
            new User { Login = "empl", UserName = "B", Role = Role.Employee }
        );
        await db.SaveChangesAsync();

        // Act
        var users = await service.GetUsersAsync();

        // Assert
        Assert.True(users.Count == 2); // только Employee
        Assert.Equal("admin", users.First().Login);
    }
}