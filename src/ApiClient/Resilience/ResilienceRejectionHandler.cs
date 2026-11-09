using Polly;
using Polly.Timeout;

namespace SSW.Rewards.ApiClient.Resilience;

/// <summary>
/// Surfaces the resilience pipeline's own failures (attempt/total timeout, open circuit) as
/// <see cref="HttpRequestException"/>, the type callers already catch for network failures.
/// Without it a Polly <see cref="ExecutionRejectedException"/> escapes those catch blocks.
/// </summary>
internal sealed class ResilienceRejectionHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            return await base.SendAsync(request, cancellationToken);
        }
        catch (ExecutionRejectedException ex)
        {
            var message = ex is TimeoutRejectedException
                ? $"{request.Method} {request.RequestUri?.AbsolutePath} timed out."
                : $"{request.Method} {request.RequestUri?.AbsolutePath} was not sent: the API is failing, so requests are paused briefly.";

            throw new HttpRequestException(message, ex);
        }
    }
}
