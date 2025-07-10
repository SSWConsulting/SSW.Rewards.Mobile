using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Notifications;

namespace SSW.Rewards.ApiClient.Services;

public interface INotificationsService
{
    Task UploadDeviceToken(DeviceTokenDto command, CancellationToken cancellationToken);
    Task SendAdminNotification(SendAdminNotificationDto command, CancellationToken cancellationToken);
    Task<NotificationHistoryListViewModel> GetNotificationHistoryListAsync(int page, int pageSize, string? search, string? sortLabel, string? sortDirection, bool includeDeleted, CancellationToken cancellationToken);
    Task ArchiveNotificationAsync(int id, CancellationToken cancellationToken);
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

    public async Task SendAdminNotification(SendAdminNotificationDto command, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}SendAdminNotification", command, cancellationToken);

        if (result.IsSuccessStatusCode)
        {
            return;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to send admin notification: {responseContent}");
    }

    public async Task<NotificationHistoryListViewModel> GetNotificationHistoryListAsync(int page, int pageSize, string? search, string? sortLabel, string? sortDirection, bool includeDeleted, CancellationToken cancellationToken)
    {
        var url = $"{_baseRoute}List?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"&search={Uri.EscapeDataString(search)}";
        }
        if (!string.IsNullOrWhiteSpace(sortLabel))
        {
            url += $"&sortLabel={Uri.EscapeDataString(sortLabel)}";
        }
        if (!string.IsNullOrWhiteSpace(sortDirection))
        {
            url += $"&sortDirection={Uri.EscapeDataString(sortDirection)}";
        }
        if (includeDeleted)
        {
            url += "&includeDeleted=true";
        }
        var result = await _httpClient.GetAsync(url, cancellationToken);
        if (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<NotificationHistoryListViewModel>(cancellationToken: cancellationToken);
            if (response is not null)
            {
                return response;
            }
        }
        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to get notification history: {responseContent}");
    }

    public async Task ArchiveNotificationAsync(int id, CancellationToken cancellationToken)
    {
        var result = await _httpClient.DeleteAsync($"{_baseRoute}DeleteNotification/{id}", cancellationToken);
        if (!result.IsSuccessStatusCode)
        {
            var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Failed to archive notification: {responseContent}");
        }
    }
}