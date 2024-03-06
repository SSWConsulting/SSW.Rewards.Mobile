using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SSW.Rewards.Application.AddressLookup;
using SSW.Rewards.Infrastructure.Options;
using SSW.Rewards.Shared.DTOs.AddressTypes;

namespace SSW.Rewards.Infrastructure.Services;

public class AddressLookupService : IAddressLookupService
{
    private string _key;
    private const string _baseUrl = "https://atlas.microsoft.com/search/address/json?&api-version=1.0&language=en-AU&countrySet=AU&typeahead=true";
    private readonly HttpClient _httpClient;
    
    public AddressLookupService(IOptions<AzureMapsOptions> options, IHttpClientFactory clientFactory)
    {
        _key = options.Value.Key;
        _httpClient = clientFactory.CreateClient();
    }
    
    public async Task<IEnumerable<Address>> Search(string query)
    {
        var requestUri = $"{_baseUrl}&subscription-key={_key}&query={query}";
        
        var resultSet = await _httpClient.GetFromJsonAsync<Root>(requestUri);
        
        return resultSet?.results.Select(r => r.address)?? Enumerable.Empty<Address>();
    }
}