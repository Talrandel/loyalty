using LoyaltyApp.Models;

namespace LoyaltyApp.Services;

public interface ICustomerService
{
    Task CreateAsync(int actingUserId, Customer customer);
    Task IncreaseBalance(int actingUserId, Customer customer, decimal amount);
    Task WithdrawBalance(int actingUserId, Customer customer, decimal amount);
    Task EditAsync(int actingUserId, Customer customer);
    Task<Customer> GetCustomer(int id);
    Task<List<Customer>> GetCustomers();
}