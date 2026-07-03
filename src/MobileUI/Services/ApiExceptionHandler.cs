namespace SSW.Rewards.Mobile.Services;

/// <summary>
/// MAUI-backed <see cref="IApiExceptionHandler"/> delegating to the existing static
/// <see cref="ExceptionHandler"/> (e.g. 401 → re-login redirect).
/// </summary>
public sealed class ApiExceptionHandler : IApiExceptionHandler
{
    public Task<bool> TryHandleAsync(Exception exception) => ExceptionHandler.HandleApiException(exception);
}
