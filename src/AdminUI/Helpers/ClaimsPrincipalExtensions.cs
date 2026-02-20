using System.Security.Claims;

namespace SSW.Rewards.Admin.UI.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static string GetDisplayName(this ClaimsPrincipal user)
    {
        // Try to get a clean display name from various claim sources
        var givenName = user.FindFirst("given_name")?.Value
                     ?? user.FindFirst(ClaimTypes.GivenName)?.Value;

        if (!string.IsNullOrEmpty(givenName))
        {
            return givenName;
        }

        var name = user.Identity?.Name;
        if (string.IsNullOrEmpty(name))
        {
            return "User";
        }

        // Handle array-like format: ["First","Last","Full Name"]
        if (name.StartsWith("[") && name.Contains(","))
        {
            // Try to extract the last element which is usually the full display name
            var parts = name.Trim('[', ']').Split(',');
            if (parts.Length > 0)
            {
                var lastPart = parts[^1].Trim().Trim('"');
                // If the last part contains brackets (like "Name [Company]"), extract just the name
                var bracketIndex = lastPart.IndexOf('[');
                if (bracketIndex > 0)
                {
                    lastPart = lastPart[..bracketIndex].Trim();
                }
                // Extract first name only for greeting
                var firstName = lastPart.Split(' ')[0];
                return firstName;
            }
        }

        // If it's a regular name, just use the first part
        return name.Split(' ')[0];
    }

    public static string GetUserRole(this ClaimsPrincipal user)
    {
        if (user.IsInRole("Admin"))
            return "Admin";
        if (user.IsInRole("Staff"))
            return "Staff";
        return "User";
    }
}
