using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.AddressTypes;

namespace SSW.Rewards.ApiClient.Services;

public interface IAddressService
{
    Task<IEnumerable<Address>> Search(string query);
}

public class AddressService : IAddressService
{
    private readonly HttpClient _httpClient;
    
    private const string _baseRoute = "api/Address/";
    
    public AddressService(IHttpClientFactory clientFactory)
    {
        _httpClient = clientFactory.CreateClient(Constants.AuthenticatedClient);
    }
    
    public async Task<IEnumerable<Address>> Search(string query)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}Search?query={query}");
        
        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<IEnumerable<Address>>();
            
            if (response is not null)
            {
                return response;
            }
        }
        
        var responseContent = await result.Content.ReadAsStringAsync();
        throw new Exception($"Failed to search for address: {responseContent}");
    }
}