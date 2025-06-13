using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection.Notifications.Commands.UploadDeviceToken;
using SSW.Rewards.Application.Notifications.Commands;

namespace SSW.Rewards.ApiClient.Services;

public interface INotificationsService
{
    Task UploadDeviceToken(DeviceTokenDto command, CancellationToken cancellationToken);
    Task SendAdminNotification(SendAdminNotificationCommand command, CancellationToken cancellationToken);
}

public class NotificationsService : INotificationsService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/Notifications/";

    public NotificationsService(IHttpClientFactory clientFactory)
    {
        _httpClient = clientFactory.CreateClient(Constants.AuthenticatedClient);
    }

    public async Task UploadDeviceToken(DeviceTokenDto dto, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PutAsJsonAsync($"{_baseRoute}UploadDeviceToken", dto, cancellationToken);

        if (result.IsSuccessStatusCode)
        {
            return;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to upload Android Device Token: {responseContent}");
    }

    public async Task SendAdminNotification(SendAdminNotificationCommand command, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}SendAdminNotification", command, cancellationToken);

        if (result.IsSuccessStatusCode)
        {
            return;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to send admin notification: {responseContent}");
    }
}