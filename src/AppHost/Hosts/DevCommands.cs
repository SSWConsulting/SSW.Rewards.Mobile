using Microsoft.Extensions.DependencyInjection;

namespace SSW.Rewards.AppHost.Hosts;

// Dashboard command buttons for the database + tooling chores, grouped on the SQL
// resource. Mobile/Tailscale commands live on the virtual mobile resource (MobileHost).
public static class DevCommands
{
    public static IResourceBuilder<SqlServerServerResource> AddDevCommands(
        this IResourceBuilder<SqlServerServerResource> sql)
    {
        var builder = sql.ApplicationBuilder;

        // Repo paths (AppHost lives in src/AppHost).
        var repoRoot = CommandHelpers.RepoRoot(builder);
        var efProject = Path.Combine(repoRoot, "src", "Infrastructure");
        var startup = Path.Combine(repoRoot, "src", "WebAPI");

        sql.WithCommand("ef-migrate", "DB: Apply migrations",
            executeCommand: async ctx =>
            {
                var (exit, log) = await CommandHelpers.RunProcess(ctx, "dotnet",
                    $"ef database update --project \"{efProject}\" --startup-project \"{startup}\"");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: new CommandOptions
            {
                Description = "dotnet ef database update against the dev SQL container",
                IconName = "DatabaseArrowUp",
                ConfirmationMessage = "Apply pending EF migrations to ssw.rewards?"
            });

        sql.WithCommand("ef-add-migration", "DB: Add migration…",
            executeCommand: async ctx =>
            {
                var name = await CommandHelpers.PromptText(ctx, "Add EF migration", "Name the new migration", "Migration name");
                if (name is null) return CommandResults.Canceled();
                name = name.Trim();
                if (string.IsNullOrWhiteSpace(name))
                    return CommandResults.Failure("Migration name is required.");
                var (exit, log) = await CommandHelpers.RunProcess(ctx, "dotnet",
                    $"ef migrations add \"{name}\" --project \"{efProject}\" --startup-project \"{startup}\"");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: new CommandOptions { IconName = "Add", Description = "dotnet ef migrations add <name>" });

        sql.WithCommand("install-ef", "Tools: Install/upgrade dotnet-ef",
            executeCommand: async ctx =>
            {
                var (exit, log) = await CommandHelpers.RunProcess(ctx, "dotnet", "tool update dotnet-ef --global");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: new CommandOptions { IconName = "Wrench", Description = "dotnet tool update dotnet-ef -g" });

        sql.WithCommand("install-aspire", "Tools: Install/upgrade Aspire CLI",
            executeCommand: async ctx =>
            {
                // `tool update` installs the tool if it's missing, so this doubles as first-time install.
                var (exit, log) = await CommandHelpers.RunProcess(ctx, "dotnet", "tool update aspire --global");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: new CommandOptions { IconName = "Wrench", Description = "dotnet tool update aspire -g (needs ≥ 13.4.6)" });

        sql.WithCommand("dev-cert", "Tools: Trust dev HTTPS cert",
            executeCommand: async ctx =>
            {
                var (exit, log) = await CommandHelpers.RunProcess(ctx, "dotnet", "dev-certs https --trust");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: new CommandOptions { IconName = "Certificate", Description = "dotnet dev-certs https --trust" });

        sql.WithCommand("aspire-doctor", "Tools: Diagnose (aspire doctor)",
            executeCommand: async ctx =>
            {
                // Read-only environment check (SDK / Docker / cert / CLI version) — output lands in the log.
                var (exit, log) = await CommandHelpers.RunProcess(ctx, "aspire", "doctor --non-interactive --nologo");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: new CommandOptions { IconName = "Stethoscope", Description = "aspire doctor — verify SDK, Docker, dev cert, CLI version" });

        return sql;
    }
}
