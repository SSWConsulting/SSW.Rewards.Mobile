using System.Security.Claims;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;
using SSW.Rewards.WebAPI.Filters;
using ClientConstants = SSW.Rewards.ApiClient.Constants;

namespace SSW.Rewards.WebAPI.Telemetry;

public class WebApiTelemetryInitializer : ITelemetryInitializer
{
    private const string ClientAppNameKey = "ClientAppName";
    private const string ClientIpKey = "ClientIp";
    private const string UserKey = "UserEmail";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TelemetryConfig _telemetryConfig;

    public WebApiTelemetryInitializer(IHttpContextAccessor httpContextAccessor, IOptions<TelemetryConfig> telemetryConfig)
    {
        _httpContextAccessor = httpContextAccessor;
        _telemetryConfig = telemetryConfig.Value;
    }

    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is RequestTelemetry req && req.ResponseCode == "404")
        {
            req.Properties["Handled404"] = "true";
            if (req.Url != null && req.Url.PathAndQuery != null && UrlBlockList.IsBlocked(req.Url.PathAndQuery))
            {
                req.Properties["BlockedUrl"] = "true";
                req.Success = true;
            }
            else
            {
                req.Properties["BlockedUrl"] = "false";
            }
        }

        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            // Initialize happens for each log. We can cache values for the HttpClient to avoid unnecessary allocations and CPU usage.
            string? clientName = GetOrCacheValue(context, ClientAppNameKey, () => GetClientAppName(context));
            telemetry.Context.GlobalProperties[ClientConstants.RewardsAppClientNameHeaderKey] = clientName;

            if (_telemetryConfig.LogIP)
            {
                string? ip = GetOrCacheValue(context, ClientIpKey, () => GetClientIp(context));
                telemetry.Context.Location.Ip = ip;
            }

            if (_telemetryConfig.LogUser)
            {
                string? user = GetOrCacheValue(context, UserKey, () => GetUser(context));
                if (user != null)
                {
                    telemetry.Context.User.AuthenticatedUserId = user;
                }
            }
        }
    }

    private static string? GetOrCacheValue(HttpContext context, string key, Func<string?> valueFactory)
    {
        if (!context.Items.TryGetValue(key, out var value))
        {
            value = valueFactory();
            context.Items[key] = value;
        }

        return value?.ToString();
    }

    private static string GetClientAppName(HttpContext context)
    {
        var clientName = context.Request.Headers.TryGetValue(ClientConstants.RewardsAppClientNameHeaderKey, out var customHeaderValue)
            ? customHeaderValue.ToString()
            : null;

        // If the client name is missing, determine if the request is from a browser or direct
        if (string.IsNullOrWhiteSpace(clientName))
        {
            var hasOrigin = context.Request.Headers.ContainsKey("Origin");
            var hasUserAgent = context.Request.Headers.ContainsKey("User-Agent");

            // Try to determine if it's used via a browser or called directly
            clientName = hasOrigin && hasUserAgent ? "Browser" : "Direct";
        }

        return clientName;
    }


    private static string? GetClientIp(HttpContext context)
        => context.Connection.RemoteIpAddress?.ToString();

    private static string? GetUser(HttpContext context)
        => context.User.Identity is ClaimsIdentity { IsAuthenticated: true } user
            ? (user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value?.ToLower())
            : null;
}

