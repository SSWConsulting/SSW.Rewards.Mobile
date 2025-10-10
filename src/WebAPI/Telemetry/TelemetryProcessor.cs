using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace SSW.Rewards.WebAPI.Telemetry;

public class TelemetryProcessor(ITelemetryProcessor next) : ITelemetryProcessor
{
    private const string HangfireStagingDbIdentifier = "db-sswrewards-hangfire";
    private const string HealthOperationName = "GET /health";

    public void Process(ITelemetry item)
    {
        if (item is DependencyTelemetry { Type: "SQL" } dep)
        {
            if (dep.Target?.Contains(HangfireStagingDbIdentifier, StringComparison.OrdinalIgnoreCase) == true)
            {
                return; // suppress telemetry for Hangfire staging DB noise
            }

            if (dep.Context.Operation.Name == HealthOperationName)
            {
                return; // suppress telemetry for health check
            }
        }
        next.Process(item);
    }
}