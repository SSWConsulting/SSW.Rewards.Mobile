using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace SSW.Rewards.WebAPI.Telemetry;

public class TelemetryProcessor(ITelemetryProcessor next) : ITelemetryProcessor
{
    private const string HangfireStagingDbIdentifier = "db-sswrewards-hangfire-staging";

    public void Process(ITelemetry item)
    {
        if (item is DependencyTelemetry { Type: "SQL" } dep)
        {
            var data = dep.Data ?? string.Empty;
            var target = dep.Target ?? string.Empty;
            var name = dep.Name ?? string.Empty;

            if (data.Contains(HangfireStagingDbIdentifier, StringComparison.OrdinalIgnoreCase)
                || target.Contains(HangfireStagingDbIdentifier, StringComparison.OrdinalIgnoreCase)
                || name.Contains(HangfireStagingDbIdentifier, StringComparison.OrdinalIgnoreCase))
            {
                return; // suppress telemetry for Hangfire staging DB noise
            }
        }
        next.Process(item);
    }
}