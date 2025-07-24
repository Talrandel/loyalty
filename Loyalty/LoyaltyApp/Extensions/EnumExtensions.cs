using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace LoyaltyApp.Extensions;

public static class EnumExtensions
{
    public static string GetEnumDisplayName(this Enum value)
    {
        return value.GetType().GetMember(value.ToString())
            .FirstOrDefault()
            ?.GetCustomAttribute<DisplayAttribute>()
            ?.Name ?? value.ToString();
    }
}