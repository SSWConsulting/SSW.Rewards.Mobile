using Microsoft.Extensions.DependencyInjection;

namespace SSW.Rewards.AppHost.Hosts;

// Dashboard command buttons for the database + tooling chores, grouped on the SQL
// resource. Mobile/Tailscale commands live on the virtual mobile resource (MobileHost).
public static class DevCommands
{
    public static IResourceBuilder<SqlServerServerResource> AddDevCommands(
        this IResourceBuilder<SqlServerServerResource> sql, CommonInfra common)
    {
        var builder = sql.ApplicationBuilder;

        // Repo paths (AppHost lives in src/AppHost).
        var repoRoot = CommandHelpers.RepoRoot(builder);
        var efProject = Path.Combine(repoRoot, "src", "Infrastructure");
        var startup = Path.Combine(repoRoot, "src", "WebAPI");
        var seederProject = Path.Combine(repoRoot, "tools", "DataSeeder");

        // The DataSeeder reads these env keys as its connection fallbacks.
        async Task<Dictionary<string, string?>> SeederEnv(ExecuteCommandContext ctx) => new()
        {
            ["ConnectionStrings__DefaultConnection"] =
                await common.RewardsDatabase.Resource.ConnectionStringExpression.GetValueAsync(ctx.CancellationToken),
            ["CloudBlobProviderOptions__ContentStorageConnectionString"] =
                await common.Blobs.Resource.ConnectionStringExpression.GetValueAsync(ctx.CancellationToken),
        };

        sql.WithCommand("db-seed", "DB: Seed demo data",
            executeCommand: async ctx =>
            {
                var email = await CommandHelpers.PromptText(ctx, "Seed demo data",
                    "The seeder pre-creates YOUR user (with history) so the app binds to your login.", "Your dev email");
                if (email is null) return CommandResults.Canceled();
                email = email.Trim();
                if (string.IsNullOrWhiteSpace(email)) return CommandResults.Failure("Dev email is required.");
                var (exit, log) = await CommandHelpers.RunProcess(ctx, "dotnet",
                    $"run --project \"{seederProject}\" --verbosity quiet -- seed --dev-email \"{email}\"",
                    await SeederEnv(ctx));
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: new CommandOptions
            {
                Description = "Idempotent Northwind demo data — users, avatars, years of scans/events, rewards. Re-run any time to top up.",
                IconName = "DatabaseMultiple"
            });

        sql.WithCommand("db-reset", "DB: Reset + reseed",
            executeCommand: async ctx =>
            {
                var email = await CommandHelpers.PromptText(ctx, "Reset + reseed",
                    "Databases are DROPPED and re-created from migrations, then seeded. Enter your dev email.", "Your dev email");
                if (email is null) return CommandResults.Canceled();
                email = email.Trim();
                if (string.IsNullOrWhiteSpace(email)) return CommandResults.Failure("Dev email is required.");
                var (exit, log) = await CommandHelpers.RunProcess(ctx, "dotnet",
                    $"run --project \"{seederProject}\" --verbosity quiet -- reset --yes --dev-email \"{email}\"",
                    await SeederEnv(ctx));
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: new CommandOptions
            {
                Description = "Drop ssw.rewards + hangfire → migrate → seed demo data. Restart rewards-webapi afterwards.",
                IconName = "ArrowClockwiseDashes",
                ConfirmationMessage = "DROP the local databases and reseed from scratch? (Your local data is lost.)"
            });

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
