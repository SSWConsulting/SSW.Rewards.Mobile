using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SSW.Rewards.Helpers
{
    public static class AuthenticatedClientFactory
    {
        private static HttpClient _httpClient;

        public static HttpClient GetClient()
        {
            if (_httpClient == null)
                _httpClient = new HttpClient();

            string token = App.Constants.AccessToken;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Console.WriteLine("[AuthenticatedClientFactory]: Setting access token:");
            Console.WriteLine(token);

            return _httpClient;
        }
    }
}
