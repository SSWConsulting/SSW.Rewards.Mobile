using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace SSW.Rewards.WebAPI.Telemetry;

public abstract class TelemetryProcessor(ITelemetryProcessor next) : ITelemetryProcessor
{
    public void Process(ITelemetry item)
    {
        if (item is DependencyTelemetry dep)
        {
            // Filter SQL dependencies related to HangFire Job Queue checks
            if (dep is { Type: "SQL", Data: not null } && dep.Data.IndexOf("[HangFire].JobQueue", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return;
            }
        }
        next.Process(item);
    }
}