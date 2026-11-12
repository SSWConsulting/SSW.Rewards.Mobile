using System.Linq.Expressions;
using SSW.Rewards.Shared.Utils;

namespace SSW.Rewards.Application.Common.Helpers;

/// <summary>
/// The raw inputs to a user's display title, as read from the database.
/// </summary>
public sealed record UserTitleSource(int UserId, string? StaffTitle, string? CompanyLink);

/// <summary>
/// The single rule for the title shown under a user's name on the profile, in the Activity feed and on the leaderboard:
/// the title on their staff record, else their Company link, else "Community".
/// </summary>
public static class UserTitles
{
    public const string Community = "Community";
    public const string CompanyPlatformName = "Company";

    /// <summary>
    /// Projects a user to the inputs of their title. Translates to two correlated sub-queries, so it can be used for any
    /// number of users in one round trip.
    /// A staff record matches by email and counts only while it has a staff achievement (soft-deleted staff records and
    /// achievements are excluded by the global query filters).
    /// </summary>
    public static Expression<Func<User, UserTitleSource>> SourceProjection(IQueryable<StaffMember> staffMembers)
        => u => new UserTitleSource(
            u.Id,
            staffMembers
                .Where(s => s.StaffAchievement != null && s.Email != null && s.Email == u.Email)
                .OrderBy(s => s.Id)
                .Select(s => s.Title)
                .FirstOrDefault(),
            u.SocialMediaIds
                .Where(s => s.SocialMediaPlatform.Name == CompanyPlatformName)
                .Select(s => s.SocialMediaUserId)
                .FirstOrDefault());

    /// <summary>
    /// Resolves the titles of the given users in a single query. The key is the user id.
    /// </summary>
    public static async Task<Dictionary<int, string>> GetUserTitlesAsync(
        this IApplicationDbContext dbContext,
        IQueryable<User> users,
        CancellationToken cancellationToken)
    {
        var sources = await users
            .AsNoTracking()
            .TagWithContext()
            .Select(SourceProjection(dbContext.StaffMembers))
            .ToListAsync(cancellationToken);

        return sources
            .GroupBy(s => s.UserId)
            .ToDictionary(g => g.Key, g => Resolve(g.First()));
    }

    /// <summary>
    /// Resolves the title of a single user, or "Community" when the user does not exist.
    /// </summary>
    public static async Task<string> GetUserTitleAsync(
        this IApplicationDbContext dbContext,
        int userId,
        CancellationToken cancellationToken)
    {
        var titles = await dbContext.GetUserTitlesAsync(dbContext.Users.Where(u => u.Id == userId), cancellationToken);
        return titles.GetValueOrDefault(userId, Community);
    }

    public static string Resolve(UserTitleSource source) => Resolve(source.StaffTitle, source.CompanyLink);

    public static string Resolve(string? staffTitle, string? companyLink)
    {
        if (!string.IsNullOrWhiteSpace(staffTitle))
        {
            return staffTitle.Trim();
        }

        var company = FormatCompany(companyLink);
        return string.IsNullOrWhiteSpace(company) ? Community : company;
    }

    /// <summary>
    /// Company links are stored as URLs (e.g. https://www.northwind.com/about). Show the domain (northwind.com);
    /// anything that is not a URL is shown as entered.
    /// </summary>
    private static string? FormatCompany(string? companyLink)
    {
        if (string.IsNullOrWhiteSpace(companyLink))
        {
            return null;
        }

        var trimmed = companyLink.Trim();
        var domain = RegexHelpers.WebsiteRegex().ExtractHandle(trimmed);

        if (string.IsNullOrEmpty(domain))
        {
            return trimmed;
        }

        return domain.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? domain[4..] : domain;
    }
}
