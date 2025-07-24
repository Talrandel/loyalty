using LoyaltyApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace LoyaltyApp.Attributes;

public class AuthorizeRoleAttribute : AuthorizeAttribute
{
    public AuthorizeRoleAttribute(Role role)
    {
        Roles = role.ToString();
    }
}