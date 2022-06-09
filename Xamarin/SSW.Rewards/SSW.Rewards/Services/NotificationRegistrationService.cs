using Newtonsoft.Json;
using SSW.Rewards.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SSW.Rewards.Services
{
    public class NotificationRegistrationService : BaseService, INotificationRegistrationService
    {
        const string RequestUrl = "api/Notifications";
        const string CachedDeviceTokenKey = "cached_device_token";
        const string CachedTagsKey = "cached_tags";

        IDeviceInstallationService _deviceInstallationService;

        public NotificationRegistrationService()
        {
            AuthenticatedClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        IDeviceInstallationService DeviceInstallationService
            => _deviceInstallationService ??
            (_deviceInstallationService = Resolver.Resolve<IDeviceInstallationService>());

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

            await SendAsync<DeviceInstall>(HttpMethod.Put, $"{RequestUrl}/UpdateInstallation", deviceInstallation).ConfigureAwait(false);

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

            var response = await AuthenticatedClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
    }
}