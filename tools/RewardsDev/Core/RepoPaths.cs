namespace SSW.Rewards.DevTool.Core;

// Resolves the repo root (the dir with SSW.Rewards.sln) and the override-file
// paths each app reads.
public sealed class RepoPaths
{
    public string Root { get; }
    public string MobileLocalFile => Path.Combine(Root, "src", "MobileUI", "Constants.LocalDev.cs");
    public string AdminLocalFile  => Path.Combine(Root, "src", "AdminUI", "wwwroot", "appsettings.Local.json");
    public string AppHostProject  => Path.Combine(Root, "src", "AppHost", "SSW.Rewards.AppHost.csproj");

    private RepoPaths(string root) => Root = root;

    // Walk up from the cwd and from the tool's own location (so it works when
    // run via `dotnet run` from anywhere in the repo).
    public static RepoPaths? Discover()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var d = new DirectoryInfo(start);
            while (d is not null)
            {
                if (File.Exists(Path.Combine(d.FullName, "SSW.Rewards.sln"))) return new RepoPaths(d.FullName);
                d = d.Parent;
            }
        }
        return null;
    }

    public string Rel(string path) => Path.GetRelativePath(Directory.GetCurrentDirectory(), path);
}
