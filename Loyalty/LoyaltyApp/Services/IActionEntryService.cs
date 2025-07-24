using LoyaltyApp.Models;

namespace LoyaltyApp.Services;

public interface IActionEntryService
{
    Task<List<ActionEntry>> GetAllAsync();
    Task<List<ActionEntry>> GetByCustomerAsync(int customerId);
    Task<List<ActionEntry>> GetByUserAsync(int userId);
}