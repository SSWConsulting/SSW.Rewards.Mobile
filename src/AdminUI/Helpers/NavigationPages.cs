using MudBlazor;

namespace SSW.Rewards.Admin.UI.Helpers;

public class NavigationPage
{
    public string Href;
    public string Icon;
    public string Title;
}

public static class NavigationPages
{
    public static List<NavigationPage> PublicPages() =>
        [
            new() { Href = "kiosk-leaderboard", Icon = Icons.Material.Filled.Leaderboard, Title = "Leaderboard (Kiosk)" }
        ];

    public static List<NavigationPage> StaffPages() =>
        [
            new() { Href = "leaderboard", Icon = Icons.Material.Filled.People, Title = "Leaderboard" },
            new() { Href = "profiles", Icon = Icons.Material.Filled.Person, Title = "Profiles" },
            new() { Href = "skills", Icon = Icons.Material.Filled.WorkspacePremium, Title = "Skills" }
        ];

    public static List<NavigationPage> AdminPages() =>
        [
            new() { Href = "achievements", Icon = Icons.Material.Filled.Dashboard, Title = "Achievements" },
            new() { Href = "rewards", Icon = Icons.Material.Filled.Redeem, Title = "Rewards" },
            new() { Href = "quizzes", Icon = Icons.Material.Filled.Quiz, Title = "Quizzes" },
            new() { Href = "users", Icon = Icons.Material.Filled.People, Title = "Users" },
            new() { Href = "newusers", Icon = Icons.Material.Filled.SupervisedUserCircle, Title = "New Users" },
            new() { Href = "prizedraw", Icon = Icons.Material.Filled.Celebration, Title = "Prize Draw" },
            new() { Href = "notifications", Icon = Icons.Material.Filled.Chat, Title = "Notifications" },
            new() { Href = "delete", Icon = Icons.Material.Filled.Delete, Title = "Delete" }
        ];
}