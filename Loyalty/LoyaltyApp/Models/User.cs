namespace LoyaltyApp.Models;

public sealed class User
{
    public User()
    {

    }

    public User(int id, string userName, string password)
    {
        Id = id;
        UserName = userName;
        PasswordHash = password;
    }

    public int Id { get; set; }
    public string Login { get; set; }
    public string UserName { get; set; }
    public string PasswordHash { get; set; }
    public Role Role { get; set; } = Role.Employee;
}