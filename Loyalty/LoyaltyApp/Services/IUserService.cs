using LoyaltyApp.Models;

namespace LoyaltyApp.Services;

public interface IUserService
{
    User Validate(string userName, string password);
    Task<User> CreateAsync(string login, string userName, string password);
    Task<List<User>> GetUsersAsync();
}