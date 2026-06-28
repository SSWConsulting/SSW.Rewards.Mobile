using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SSW.Rewards.AppHost.Hosts;

// A virtual dashboard resource representing the MAUI mobile app.
//
// The mobile app is NOT orchestrated by Aspire — it runs on a device/emulator and
// talks to the backend over the network (see _docs/Aspire-Local-Dev.md). But modelling
// it as a lifetime-less resource gives it a home on the dashboard where we can hang the
// mobile dev chores: switch which API/identity it targets, flip to a Tailscale URL,
// materialize the git-ignored Firebase config, and restore the MAUI workloads.
//
// IResourceWithoutLifetime → Aspire never tries to start/stop it, so it shows as a static
// entry (like a parameter) instead of spinning forever on "Starting".
public sealed class MobileAppResource(string name) : Resource(name), IResourceWithoutLifetime;

public static class MobileHostExtensions
{
    public static IResourceBuilder<MobileAppResource> AddMobileApp(this IDistributedApplicationBuilder builder)
    {
        var repoRoot = CommandHelpers.RepoRoot(builder);
        var devTool = Path.Combine(repoRoot, "tools", "RewardsDev", "RewardsDev.csproj");
        var mobileProject = Path.Combine(repoRoot, "src", "MobileUI", "MobileUI.csproj");

        var mobile = builder.AddResource(new MobileAppResource("mobile-app"))
            .WithInitialState(new CustomResourceSnapshot
            {
                ResourceType = "MobileApp",
                State = new ResourceStateSnapshot("Runs on device/emulator", KnownResourceStateStyles.Info),
                Properties =
                [
                    new("aspire.resource.source", "src/MobileUI"),
                    new("note", "Not orchestrated by Aspire — use the commands here to point it at a backend"),
                    new("tip", "Emulators can't reach localhost — use staging or a Tailscale URL, not local"),
                ],
            });

        // Always enable command buttons — this resource has no running lifecycle.
        static CommandOptions Always(string icon, string description, string? confirm = null) => new()
        {
            IconName = icon,
            Description = description,
            ConfirmationMessage = confirm,
            UpdateState = _ => ResourceCommandState.Enabled,
        };

        mobile.WithCommand("mobile-show-target", "Show current target",
            executeCommand: async ctx =>
            {
                var (exit, log) = await CommandHelpers.RunProcess(ctx, "dotnet", $"run --project \"{devTool}\" -- show");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: Always("Info", "rewards-dev show — prints the API + identity the apps point at"));

        // Setup gate: validate / open the ONE AppHost secrets store (paste the Keeper blob).
        mobile.WithCommand("secrets-check", "Secrets: Validate",
            executeCommand: async ctx =>
            {
                var (exit, log) = await CommandHelpers.RunProcess(ctx, "dotnet", $"run --project \"{devTool}\" -- secrets check");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: Always("ShieldCheckmark", "rewards-dev secrets check — confirm every required secret is set (from Keeper)"));

        mobile.WithCommand("secrets-edit", "Secrets: Open file",
            executeCommand: async ctx =>
            {
                var (exit, log) = await CommandHelpers.RunProcess(ctx, "dotnet", $"run --project \"{devTool}\" -- secrets edit");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: Always("DocumentKey", "rewards-dev secrets edit — open secrets.json to paste the Keeper record"));

        mobile.WithCommand("mobile-switch-api", "Switch API target…",
            executeCommand: async ctx =>
            {
                var target = await CommandHelpers.PromptChoice(ctx, "Switch mobile API",
                    "Choose the API base URL for the mobile app", "Target",
                    "local", "staging", "prod", "tailscale");
                if (target is null) return CommandResults.Canceled();
                var (exit, log) = await CommandHelpers.RunProcess(ctx, "dotnet", $"run --project \"{devTool}\" -- api {target.Trim()}");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: Always("Globe", "rewards-dev api <local|staging|prod|tailscale>"));

        mobile.WithCommand("mobile-api-tailscale", "API → Tailscale (one-click)",
            executeCommand: async ctx =>
            {
                var (exit, log) = await CommandHelpers.RunProcess(ctx, "dotnet", $"run --project \"{devTool}\" -- api tailscale");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: Always("CloudLink", "rewards-dev api tailscale — point mobile at this machine's Tailscale URL + start `tailscale serve`"));

        mobile.WithCommand("mobile-switch-identity", "Switch identity target…",
            executeCommand: async ctx =>
            {
                var target = await CommandHelpers.PromptChoice(ctx, "Switch mobile identity",
                    "Choose the identity authority for the mobile app", "Target",
                    "local", "staging", "prod");
                if (target is null) return CommandResults.Canceled();
                var (exit, log) = await CommandHelpers.RunProcess(ctx, "dotnet", $"run --project \"{devTool}\" -- identity {target.Trim()}");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: Always("Person", "rewards-dev identity <local|staging|prod>"));

        mobile.WithCommand("tailscale-status", "Tailscale: Status",
            executeCommand: async ctx =>
            {
                var (exit, log) = await CommandHelpers.RunProcess(ctx, "tailscale", "status");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(
                    string.IsNullOrWhiteSpace(log) ? "Tailscale not found / not running. Install it and run `tailscale up`." : log);
            },
            commandOptions: Always("CloudCheckmark", "tailscale status — verify connectivity before using the tailscale target"));

        mobile.WithCommand("mobile-materialize-secrets", "Sync mobile secrets (isolated)",
            executeCommand: async ctx =>
            {
                // Routes through `rewards-dev secrets sync-mobile`: copies ONLY the two Firebase
                // client-config keys from the AppHost store into MobileUI's OWN isolated user-secrets
                // store, then writes the git-ignored google-services.json + GoogleService-Info.plist.
                // Backend secrets are never copied, so nothing sensitive can reach the APK.
                var (exit, log) = await CommandHelpers.RunProcess(ctx, "dotnet", $"run --project \"{devTool}\" -- secrets sync-mobile");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: Always("PhoneKey",
                "Copy ONLY mobile Firebase secrets into the isolated mobile store + write the config files",
                "Overwrite the local mobile Firebase config files?"));

        mobile.WithCommand("mobile-build-android", "Build & Run (Android)",
            executeCommand: async ctx =>
            {
                // Wraps `rewards-dev mobile android`: builds Android-only (no iOS workload needed)
                // and deploys to a running emulator/device. Make sure an emulator is up and the API
                // target is reachable (staging/tailscale) first.
                var (exit, log) = await CommandHelpers.RunProcess(ctx, "dotnet", $"run --project \"{devTool}\" -- mobile android");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: Always("PhoneArrowRight",
                "rewards-dev mobile android — build + deploy to a running Android emulator (no iOS workload needed)"));

        mobile.WithCommand("mobile-maui-restore", "MAUI workload restore",
            executeCommand: async ctx =>
            {
                var (exit, log) = await CommandHelpers.RunProcess(ctx, "dotnet", $"workload restore \"{mobileProject}\"");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: Always("PhoneLaptop", "dotnet workload restore (iOS/Android prereqs)"));

        mobile.WithCommand("mobile-workload-update", "Update .NET workloads",
            executeCommand: async ctx =>
            {
                var (exit, log) = await CommandHelpers.RunProcess(ctx, "dotnet", "workload update");
                return exit == 0 ? CommandResults.Success() : CommandResults.Failure(log);
            },
            commandOptions: Always("ArrowSync", "dotnet workload update — upgrade installed workloads (MAUI etc.)"));

        return mobile;
    }
}
