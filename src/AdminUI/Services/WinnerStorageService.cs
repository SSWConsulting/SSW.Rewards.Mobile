using Microsoft.JSInterop;
using SSW.Rewards.Admin.UI.Models;

namespace SSW.Rewards.Admin.UI.Services;

public interface IWinnerStorageService
{
    Task<List<Winner>> LoadWinnersAsync();
    Task SaveWinnerAsync(string userId, string name);
    Task ClearWinnersAsync();
}

public class WinnerStorageService(IJSRuntime jsRuntime) : IWinnerStorageService
{
    private const string PreviousWinnersKey = "ssw-rewards-previous-winners";

    public async Task<List<Winner>> LoadWinnersAsync()
    {
        var winnersJson = await jsRuntime.InvokeAsync<string>("sessionStorage.getItem", PreviousWinnersKey);
        if (string.IsNullOrEmpty(winnersJson))
            return [];

        var winners = System.Text.Json.JsonSerializer.Deserialize<List<Winner>>(winnersJson);
        return winners ?? [];
    }

    public async Task SaveWinnerAsync(string userId, string name)
    {
        var winners = await LoadWinnersAsync();
        winners.Add(new Winner { Id = userId, Name = name });
        var winnersJson = System.Text.Json.JsonSerializer.Serialize(winners);
        await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", PreviousWinnersKey, winnersJson);
    }

    public async Task ClearWinnersAsync()
    {
        await jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", PreviousWinnersKey);
    }
}