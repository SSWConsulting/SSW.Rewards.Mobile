using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// =====================================================================
// SSW.Rewards — .NET Aspire local-dev AppHost
//
// Replaces docker-compose + up.ps1 + hand-edited config for local dev.
//   • SQL Server (persistent) + Azurite, one prompted SA password
//   • All backend secrets supplied as Aspire secret PARAMETERS (stored in
//     THIS project's user-secrets, id F76E3E10-… — the WebAPI's own
//     user-secrets are no longer used; config flows in from here)
//   • Connection strings + blob string injected under the EXACT keys the
//     app binds (ConnectionStrings:DefaultConnection / :HangfireConnection,
//     CloudBlobProviderOptions:ContentStorageConnectionString)
//   • Dashboard command buttons for the common dev chores, including two
//     that shell out to the `rewards-dev` helper CLI so the same logic is
//     usable head-less (AI / scripts).
// =====================================================================

var builder = DistributedApplication.CreateBuilder(args);

// Repo root, for shelling out to the helper CLI (AppHost lives in src/AppHost).
var repoRoot = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", ".."));
var devToolProject = Path.Combine(repoRoot, "tools", "RewardsDev", "RewardsDev.csproj");

// ---------------------------------------------------------------------
// Secrets — prompted once, persisted to THIS AppHost's user-secrets.
// ---------------------------------------------------------------------
var saPassword       = builder.AddParameter("sql-sa-password", secret: true);
var firebaseCreds    = builder.AddParameter("firebase-credentials", secret: true);
var sendGridApiKey   = builder.AddParameter("sendgrid-api-key", secret: true);
var emailUser        = builder.AddParameter("email-user", secret: true);
var emailPassword    = builder.AddParameter("email-password", secret: true);
var signingAuthority = builder.AddParameter("signing-authority", secret: false);
// Mobile Firebase config files, materialized on demand by a command (see below).
var googleServicesJson = builder.AddParameter("mobile-google-services-json", secret: true);
var googleServiceInfoPlist = builder.AddParameter("mobile-google-service-info-plist", secret: true);

// ---------------------------------------------------------------------
// SQL Server: persistent container + data volume + the two databases.
// ---------------------------------------------------------------------
var sql = builder.AddSqlServer("rewards-sql", password: saPassword)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("ssw-rewards-sql-data");

var rewardsDb  = sql.AddDatabase("rewards-db", "ssw.rewards");
var hangfireDb = sql.AddDatabase("hangfire-db", "ssw.rewards.hangfire");

// ---------------------------------------------------------------------
// Azurite (Azure Storage emulator): persistent.
// ---------------------------------------------------------------------
var storage = builder.AddAzureStorage("rewards-storage")
    .RunAsEmulator(e => e.WithDataVolume("ssw-rewards-azurite-data"));
var blobs = storage.AddBlobs("blobs");

// ---------------------------------------------------------------------
// WebAPI — secrets + connection strings injected under the app's keys.
// ---------------------------------------------------------------------
var api = builder.AddProject<Projects.WebAPI>("rewards-webapi")
    .WithEnvironment("ConnectionStrings__DefaultConnection", rewardsDb)
    .WithEnvironment("ConnectionStrings__HangfireConnection", hangfireDb)
    // Use Azurite for the content blob store locally (overrides the prod string).
    .WithEnvironment("CloudBlobProviderOptions__ContentStorageConnectionString", blobs)
    .WithEnvironment("Firebase__FirebaseCredentials", firebaseCreds)
    .WithEnvironment("SendGridAPIKey", sendGridApiKey)
    .WithEnvironment("EmailUser", emailUser)
    .WithEnvironment("EmailPassword", emailPassword)
    .WithEnvironment("SigningAuthority", signingAuthority)
    .WaitFor(rewardsDb)
    .WaitFor(hangfireDb)
    .WaitFor(blobs)
    .WithExternalHttpEndpoints();

// ---------------------------------------------------------------------
// AdminUI — Blazor WASM dev server, points at the API.
// ---------------------------------------------------------------------
builder.AddProject<Projects.AdminUI>("rewards-adminui")
    .WithReference(api)
    .WaitFor(api)
    .WithExternalHttpEndpoints();

// ---------------------------------------------------------------------
// Dashboard command buttons (grouped on the SQL resource).
// ---------------------------------------------------------------------
var startup = Path.Combine(repoRoot, "src", "WebAPI");
var efProject = Path.Combine(repoRoot, "src", "Infrastructure");

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
        var name = result.Data.First(i => i.Name == "MigrationName").Value;
        var (exit, log) = await RunProcess(ctx, "dotnet",
            $"ef migrations add {name} --project \"{efProject}\" --startup-project \"{startup}\"");
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
        var json  = await googleServicesJson.Resource.GetValueAsync(ctx.CancellationToken);
        var plist = await googleServiceInfoPlist.Resource.GetValueAsync(ctx.CancellationToken);
        if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(plist))
            return CommandResults.Failure("Set the mobile-google-services-json / mobile-google-service-info-plist parameters first.");
        var androidPath = Path.Combine(repoRoot, "src", "MobileUI", "Platforms", "Android", "google-services.json");
        var iosPath     = Path.Combine(repoRoot, "src", "MobileUI", "Platforms", "iOS", "GoogleService-Info.plist");
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

// --- The two switching commands shell out to the rewards-dev helper CLI ---
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

builder.Build().Run();

// Stream a process into the dashboard resource log + return (exit, combined output).
static async Task<(int exit, string log)> RunProcess(ExecuteCommandContext ctx, string file, string args)
{
    var logger = ctx.ServiceProvider.GetRequiredService<ResourceLoggerService>().GetLogger(ctx.ResourceName);
    var psi = new ProcessStartInfo(file, args)
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };
    using var p = Process.Start(psi)!;
    var outp = await p.StandardOutput.ReadToEndAsync(ctx.CancellationToken);
    var err  = await p.StandardError.ReadToEndAsync(ctx.CancellationToken);
    await p.WaitForExitAsync(ctx.CancellationToken);
    logger.LogInformation("{Out}{Err}", outp, err);
    return (p.ExitCode, outp + err);
}
