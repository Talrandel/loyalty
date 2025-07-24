namespace LoyaltyApp.Models;

public sealed class ActionEntry
{
    public int Id { get; set; }

    public ActionType ActionType { get; set; }

    /// <summary>
    /// Кто совершил действие.
    /// </summary>
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// Кто совершил действие.
    /// </summary>
    public int? CustomerId { get; set; }
    public Customer Customer { get; set; }

    /// <summary>
    /// Для Increase/Withdraw.
    /// </summary>
    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    /// <summary>
    /// Опционально: для пояснения, что произошло.
    /// </summary>
    public string Description { get; set; }
}