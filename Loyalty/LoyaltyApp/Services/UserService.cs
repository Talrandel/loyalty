using LoyaltyApp.Db;
using LoyaltyApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyApp.Services;

internal sealed class UserService : IUserService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IPasswordHasher<User> _hasher;

    public UserService(
        IPasswordHasher<User> hasher,
        IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _hasher = hasher;
        _dbFactory = dbFactory;
    }

    public User Validate(string login, string password)
    {
        using var dbContext = _dbFactory.CreateDbContext();
        var user = dbContext.Users
            .AsNoTracking()
            .FirstOrDefault(u => u.Login == login);
        if (user is null)
        {
            return null;
        }

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Success ? user : null;
    }

    public async Task<User> CreateAsync(int actingUserId, string login, string userName, string password)
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        if (await dbContext.Users.AnyAsync(u => u.Login == login))
        {
            throw new InvalidOperationException("Пользователь уже существует");
        }

        var user = new User { Login = login, UserName = userName! };
        user.PasswordHash = _hasher.HashPassword(user, password!);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return user;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        return await dbContext.Users
            .ToListAsync();
    }

    public async Task UpdateUserAsync(int actingUserId, User user)
    {
        await using var dbContext = await _dbFactory.CreateDbContextAsync();
        var entity = await dbContext.Users.FindAsync(user.Id);
        if (entity != null)
        {
            var oldRole = entity.Role;
            entity.Role = user.Role;
            await dbContext.SaveChangesAsync();

            dbContext.ActionEntries.Add(new ActionEntry
            {
                ActionType = ActionType.ChangeUserRole,
                UserId = actingUserId,
                Date = DateTime.UtcNow,
                Description = $"Роль изменена с {oldRole} на {user.Role}"
            });

            await dbContext.SaveChangesAsync();
        }
    }
}