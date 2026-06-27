using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SSW.Rewards.AppHost.Hosts;

// Shared helpers for the dashboard command buttons: repo path discovery, a process
// runner that streams into the resource log, and a single-text-input prompt.
internal static class CommandHelpers
{
    // The AppHost lives in src/AppHost, so the repo root is two levels up.
    public static string RepoRoot(IDistributedApplicationBuilder builder)
        => Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", ".."));

    // Prompt for a single line of text. Returns null when the user cancels.
    public static async Task<string?> PromptText(ExecuteCommandContext ctx, string title, string message, string label)
    {
        var interaction = ctx.ServiceProvider.GetRequiredService<IInteractionService>();
        var result = await interaction.PromptInputsAsync(title, message,
            [new InteractionInput { Name = "value", Label = label, InputType = InputType.Text }]);
        if (result.Canceled) return null;
        return result.Data.First(i => i.Name == "value").Value;
    }

    // Run a process, append its combined output to the dashboard resource log,
    // and return (exit, combined output).
    public static async Task<(int exit, string log)> RunProcess(ExecuteCommandContext ctx, string file, string args)
    {
        var logger = ctx.ServiceProvider.GetRequiredService<ResourceLoggerService>().GetLogger(ctx.ResourceName);
        var psi = new ProcessStartInfo(file, args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        Process? p;
        try
        {
            p = Process.Start(psi);
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to start {File}: {Error}", file, ex.Message);
            return (-1, $"could not start {file}: {ex.Message}");
        }

        if (p is null)
        {
            var msg = $"could not start {file}";
            logger.LogError("{Error}", msg);
            return (-1, msg);
        }

        using (p)
        {
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
}
