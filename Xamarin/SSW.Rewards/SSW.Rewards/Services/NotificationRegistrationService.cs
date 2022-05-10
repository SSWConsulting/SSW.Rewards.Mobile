using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Xamarin.Essentials;

namespace SSW.Rewards.Services
{
    public class NotificationRegistrationService : INotificationRegistrationService
    {
        const string RequestUrl = "api/Notifications";
        const string CachedDeviceTokenKey = "cached_device_token";
        const string CachedTagsKey = "cached_tags";

        HttpClient _httpClient;
        IDeviceInstallationService _deviceInstallationService;
        //private NotificationsClient _notificationsClient;

        public NotificationRegistrationService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            //_notificationsClient = new NotificationsClient(App.Constants.ApiBaseUrl, _httpClient);
        }

        IDeviceInstallationService DeviceInstallationService
            => _deviceInstallationService ??
            (_deviceInstallationService = ServiceContainer.Resolve<IDeviceInstallationService>());

        public async Task DeregisterDeviceAsync()
        {
            var cachedToken = await SecureStorage.GetAsync(CachedDeviceTokenKey)
                .ConfigureAwait(false);

            if (cachedToken == null)
                return;

            var deviceId = DeviceInstallationService?.GetDeviceId();

            if (string.IsNullOrWhiteSpace(deviceId))
                throw new Exception("Unable to resolve an ID for the device.");

            await SendAsync(HttpMethod.Delete, $"{RequestUrl}/DeleteInstallation/{deviceId}")
                .ConfigureAwait(false);

            SecureStorage.Remove(CachedDeviceTokenKey);
            SecureStorage.Remove(CachedTagsKey);
        }

        public async Task RegisterDeviceAsync(params string[] tags)
        {
            DeviceInstall deviceInstallation = DeviceInstallationService?.GetDeviceInstallation(tags);

            Console.WriteLine(deviceInstallation.InstallationId);
            //await _notificationsClient.UpdateInstallationAsync(deviceInstallation);
            try
            {
                await SendAsync<DeviceInstall>(HttpMethod.Put, $"{RequestUrl}/UpdateInstallation", deviceInstallation).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nStackTrace ---\n{0}", ex.StackTrace);
            }

            await SecureStorage.SetAsync(CachedDeviceTokenKey, deviceInstallation.PushChannel)
                .ConfigureAwait(false);

            await SecureStorage.SetAsync(CachedTagsKey, JsonConvert.SerializeObject(tags));
        }

        public async Task RefreshRegistrationAsync()
        {
            var cachedToken = await SecureStorage.GetAsync(CachedDeviceTokenKey)
                .ConfigureAwait(false);

            var serializedTags = await SecureStorage.GetAsync(CachedTagsKey)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(cachedToken) ||
                string.IsNullOrWhiteSpace(serializedTags) ||
                string.IsNullOrWhiteSpace(DeviceInstallationService.Token) ||
                cachedToken == DeviceInstallationService.Token)
                return;

            var tags = JsonConvert.DeserializeObject<string[]>(serializedTags);

            await RegisterDeviceAsync(tags);
        }

        async Task SendAsync<T>(HttpMethod requestType, string requestUri, T obj)
        {
            string serializedContent = null;

            await Task.Run(() => serializedContent = JsonConvert.SerializeObject(obj))
                .ConfigureAwait(false);

            await SendAsync(requestType, requestUri, serializedContent);
        }

        async Task SendAsync(HttpMethod requestType, string requestUri, string jsonRequest = null)
        {
            var request = new HttpRequestMessage(requestType, new Uri($"{App.Constants.ApiBaseUrl}/{requestUri}"));

            if (jsonRequest != null)
                request.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
    }
}