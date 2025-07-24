namespace LoyaltyApp.Models;

public sealed class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string PhoneLastFourDigits { get; set; }
    public string PhoneHashed { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
}