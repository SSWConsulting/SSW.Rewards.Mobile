using System.Net;

namespace SSW.Rewards.Mobile.Services;

public static class ApiExceptionExtensions
{
    /// <summary>True when the API rejected the session (401), i.e. the user must log in again.</summary>
    public static bool IsUnauthorized(this Exception exception) =>
        exception is HttpRequestException { StatusCode: HttpStatusCode.Unauthorized };
}
