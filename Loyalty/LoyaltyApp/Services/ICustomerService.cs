using LoyaltyApp.Models;

namespace LoyaltyApp.Services;

public interface ICustomerService
{
    Task CreateAsync(Customer customer);
    Task IncreaseBalance(Customer customer);
    Task WithdrawBalance(Customer customer);
    Task EditAsync(Customer customer);
}