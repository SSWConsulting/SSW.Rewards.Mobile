using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SSW.Rewards.AppHost.Hosts;

// Dashboard command buttons for the common local-dev chores, grouped on the SQL
// resource. The two switching commands shell out to the `rewards-dev` helper CLI
// so the same logic is usable head-less (AI / scripts).
public static class DevCommands
{
    public static IResourceBuilder<SqlServerServerResource> AddDevCommands(
        this IResourceBuilder<SqlServerServerResource> sql)
    {
        var builder = sql.ApplicationBuilder;

        // Repo paths (AppHost lives in src/AppHost).
        var repoRoot = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", ".."));
        var devToolProject = Path.Combine(repoRoot, "tools", "RewardsDev", "RewardsDev.csproj");
        var efProject = Path.Combine(repoRoot, "src", "Infrastructure");
        var startup = Path.Combine(repoRoot, "src", "WebAPI");

        // Mobile Firebase config files, materialized on demand by a command (below).
        var googleServicesJson = builder.AddParameter("mobile-google-services-json", secret: true);
        var googleServiceInfoPlist = builder.AddParameter("mobile-google-service-info-plist", secret: true);

        sql.WithCommand("ef-migrate", "DB: Apply migrations",
            executeCommand: async ctx =>
            {
                var (exit, log) = await RunProcess(ctx, "dotnet",
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
                var interaction = ctx.ServiceProvider.GetRequiredService<IInteractionService>();
                var result = await interaction.PromptInputsAsync("Add EF migration", "Name the new migration",
                    [new InteractionInput { Name = "MigrationName", Label = "Migration name", InputType = InputType.Text }]);
                if (result.Canceled) return CommandResults.Canceled();
                var name = result.Data.First(i => i.Name == "MigrationName").Value?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                    return CommandResults.Failure("Migration name is required.");
                var (exit, log) = await RunProcess(ctx, "dotnet",
                    $"ef migrations add \"{name}\" --project \"{efProject}\" --startup-project \"{startup}\"");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: new CommandOptions { IconName = "Add", Description = "dotnet ef migrations add <name>" });

        sql.WithCommand("install-ef", "Tools: Install/upgrade dotnet-ef",
            executeCommand: async ctx =>
            {
                var (exit, log) = await RunProcess(ctx, "dotnet", "tool update dotnet-ef --global");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: new CommandOptions { IconName = "Wrench", Description = "dotnet tool update dotnet-ef -g" });

        sql.WithCommand("maui-restore", "Tools: MAUI workload restore",
            executeCommand: async ctx =>
            {
                var (exit, log) = await RunProcess(ctx, "dotnet", $"workload restore \"{Path.Combine(repoRoot, "src", "MobileUI", "MobileUI.csproj")}\"");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: new CommandOptions { IconName = "PhoneLaptop", Description = "dotnet workload restore (iOS/Android prereqs)" });

        sql.WithCommand("dev-cert", "Tools: Trust dev HTTPS cert",
            executeCommand: async ctx =>
            {
                var (exit, log) = await RunProcess(ctx, "dotnet", "dev-certs https --trust");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: new CommandOptions { IconName = "Certificate", Description = "dotnet dev-certs https --trust" });

        sql.WithCommand("materialize-mobile-secrets", "Mobile: Materialize Firebase secrets",
            executeCommand: async ctx =>
            {
                var logger = ctx.ServiceProvider.GetRequiredService<ResourceLoggerService>().GetLogger(ctx.ResourceName);
                var json = await googleServicesJson.Resource.GetValueAsync(ctx.CancellationToken);
                var plist = await googleServiceInfoPlist.Resource.GetValueAsync(ctx.CancellationToken);
                if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(plist))
                    return CommandResults.Failure("Set the mobile-google-services-json / mobile-google-service-info-plist parameters first.");
                var androidPath = Path.Combine(repoRoot, "src", "MobileUI", "Platforms", "Android", "google-services.json");
                var iosPath = Path.Combine(repoRoot, "src", "MobileUI", "Platforms", "iOS", "GoogleService-Info.plist");
                await File.WriteAllTextAsync(androidPath, json, ctx.CancellationToken);
                await File.WriteAllTextAsync(iosPath, plist, ctx.CancellationToken);
                logger.LogInformation("Wrote {Android} and {Ios}", androidPath, iosPath);
                return CommandResults.Success();
            },
            commandOptions: new CommandOptions
            {
                IconName = "PhoneKey",
                Description = "Write google-services.json + GoogleService-Info.plist from the secret parameters",
                ConfirmationMessage = "Overwrite the local mobile Firebase config files?"
            });

        sql.WithCommand("switch-identity", "Mobile: Switch identity target…",
            executeCommand: async ctx =>
            {
                var interaction = ctx.ServiceProvider.GetRequiredService<IInteractionService>();
                var result = await interaction.PromptInputsAsync("Switch mobile identity", "Choose the identity authority for the mobile app",
                    [new InteractionInput { Name = "Target", Label = "Target (local | staging | prod)", InputType = InputType.Text }]);
                if (result.Canceled) return CommandResults.Canceled();
                var target = result.Data.First(i => i.Name == "Target").Value;
                var (exit, log) = await RunProcess(ctx, "dotnet", $"run --project \"{devToolProject}\" -- identity {target}");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: new CommandOptions { IconName = "Person", Description = "rewards-dev identity <local|staging|prod>" });

        sql.WithCommand("switch-api", "Mobile: Switch API target…",
            executeCommand: async ctx =>
            {
                var interaction = ctx.ServiceProvider.GetRequiredService<IInteractionService>();
                var result = await interaction.PromptInputsAsync("Switch mobile API", "Choose the API base URL for the mobile app",
                    [new InteractionInput { Name = "Target", Label = "Target (local | staging | prod | tailscale)", InputType = InputType.Text }]);
                if (result.Canceled) return CommandResults.Canceled();
                var target = result.Data.First(i => i.Name == "Target").Value;
                var (exit, log) = await RunProcess(ctx, "dotnet", $"run --project \"{devToolProject}\" -- api {target}");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: new CommandOptions { IconName = "Globe", Description = "rewards-dev api <local|staging|prod|tailscale>" });

        return sql;
    }

    // Run a process, append its combined output to the dashboard resource log,
    // and return (exit, combined output).
    private static async Task<(int exit, string log)> RunProcess(ExecuteCommandContext ctx, string file, string args)
    {
        var logger = ctx.ServiceProvider.GetRequiredService<ResourceLoggerService>().GetLogger(ctx.ResourceName);
        var psi = new ProcessStartInfo(file, args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        using var p = Process.Start(psi);
        if (p is null)
        {
            var msg = $"could not start {file}";
            logger.LogError("{Error}", msg);
            return (-1, msg);
        }

        // Read both streams concurrently to avoid a deadlock when the child
        // fills one buffer while we block reading the other.
        var outTask = p.StandardOutput.ReadToEndAsync(ctx.CancellationToken);
        var errTask = p.StandardError.ReadToEndAsync(ctx.CancellationToken);
        await p.WaitForExitAsync(ctx.CancellationToken);
        var outp = await outTask;
        var err = await errTask;
        logger.LogInformation("{Out}{Err}", outp, err);
        return (p.ExitCode, outp + err);
    }
}
