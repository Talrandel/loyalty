using System.ComponentModel.DataAnnotations;

namespace LoyaltyApp.Models;

public enum Role
{
    [Display(Name = "Сотрудник")]
    Employee,
    
    [Display(Name = "Администратор")]
    Admin
}