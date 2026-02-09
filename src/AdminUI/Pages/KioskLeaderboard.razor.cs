using MudBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Enums;
using SSW.Rewards.Shared.DTOs.Leaderboard;

namespace SSW.Rewards.Admin.UI.Pages;

public partial class KioskLeaderboard : IDisposable
{
    private const int DefaultPageSize = 30;
    private const int MinPageSize = 10;
    private const int MaxPageSize = 40;
    private const int RefreshIntervalSeconds = 60;
    private const int ScrollIntervalSeconds = 10;

    private MudTable<MobileLeaderboardUserDto>? table;
    private LeaderboardFilter _selectedFilter = LeaderboardFilter.ThisWeek;
    private int _activeTabIndex = 0;

    private System.Timers.Timer? _refreshTimer;
    private System.Timers.Timer? _countdownTimer;
    private System.Timers.Timer? _scrollTimer;
    private System.Timers.Timer? _progressTimer;

    private DateTime? _lastUpdated;
    private bool _hasErrorsOnUpdate;
    private int _secondsUntilRefresh = RefreshIntervalSeconds;

    private int _currentPage;
    private int _totalPages = 1;
    private double _scrollProgress;
    private int _resolvedPageSize = DefaultPageSize;

    private TableData<MobileLeaderboardUserDto> _lastTableCache = new() { TotalItems = 0, Items = [] };

    [SupplyParameterFromQuery(Name = "rows")]
    public int? Rows { get; set; }

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        // Refresh data from API every 60 seconds
        _refreshTimer = new System.Timers.Timer(RefreshIntervalSeconds * 1000);
        _refreshTimer.Elapsed += async (_, _) =>
        {
            await InvokeAsync(async () =>
            {
                _secondsUntilRefresh = RefreshIntervalSeconds;
                _currentPage = 0;
                _scrollProgress = 0;
                await LoadLeaderboard();
            });
        };
        _refreshTimer.AutoReset = true;
        _refreshTimer.Start();

        // Tick the countdown display every second
        _countdownTimer = new System.Timers.Timer(1000);
        _countdownTimer.Elapsed += (_, _) =>
        {
            InvokeAsync(() =>
            {
                if (_secondsUntilRefresh > 0) _secondsUntilRefresh--;
                StateHasChanged();
            });
        };
        _countdownTimer.AutoReset = true;
        _countdownTimer.Start();

        // Auto-scroll to next page every 10 seconds
        _scrollTimer = new System.Timers.Timer(ScrollIntervalSeconds * 1000);
        _scrollTimer.Elapsed += async (_, _) =>
        {
            await InvokeAsync(async () =>
            {
                if (_totalPages > 1)
                {
                    _currentPage = (_currentPage + 1) % _totalPages;
                    _scrollProgress = 0;
                    await LoadLeaderboard();
                }
            });
        };
        _scrollTimer.AutoReset = true;
        _scrollTimer.Start();

        // Smooth progress bar animation (100ms ticks)
        _progressTimer = new System.Timers.Timer(100);
        _progressTimer.Elapsed += (_, _) =>
        {
            InvokeAsync(() =>
            {
                if (_totalPages > 1)
                {
                    _scrollProgress = Math.Min(100, _scrollProgress + (100.0 * 100 / (ScrollIntervalSeconds * 1000)));
                }
                StateHasChanged();
            });
        };
        _progressTimer.AutoReset = true;
        _progressTimer.Start();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        await ResolvePageSizeAsync();
        await LoadLeaderboard();
    }

    private async Task<TableData<MobileLeaderboardUserDto>> ServerReload(TableState state)
    {
        try
        {
            var result = await leaderboardService.GetMobilePaginatedLeaderboard(
                _currentPage, _resolvedPageSize, _selectedFilter, CancellationToken.None);

            _lastUpdated = DateTime.Now;
            _hasErrorsOnUpdate = false;

            // Filter to users with points > 0
            var usersWithPoints = result.Items.Where(u => u.Points > 0).ToList();

            // Determine total pages: if this page has fewer users with points than PageSize,
            // this is the last meaningful page
            if (usersWithPoints.Count < _resolvedPageSize)
            {
                _totalPages = Math.Max(1, _currentPage + 1);
            }
            else
            {
                // Full page of users with points - estimate from total count
                _totalPages = Math.Max(_currentPage + 2,
                    (int)Math.Ceiling((double)result.Count / _resolvedPageSize));
            }

            _lastTableCache = new TableData<MobileLeaderboardUserDto>
            {
                TotalItems = usersWithPoints.Count,
                Items = usersWithPoints
            };
        }
        catch (Exception ex)
        {
            _hasErrorsOnUpdate = true;
            Console.WriteLine($"Error in ServerReload: {ex.Message}");
        }

        StateHasChanged();
        return _lastTableCache;
    }

    private async Task LoadLeaderboard()
    {
        if (table != null)
        {
            await table.ReloadServerData();
        }
    }

    private async Task ResolvePageSizeAsync()
    {
        if (Rows.HasValue)
        {
            _resolvedPageSize = Math.Clamp(Rows.Value, MinPageSize, MaxPageSize);
            return;
        }

        try
        {
            var viewport = await JsRuntime.InvokeAsync<ViewportSize>("kioskLeaderboard.getViewport");
            _resolvedPageSize = CalculateAutoPageSize(viewport.Width, viewport.Height);
        }
        catch
        {
            _resolvedPageSize = DefaultPageSize;
        }
    }

    private static int CalculateAutoPageSize(int width, int height)
    {
        var rows = height switch
        {
            >= 1800 => 35,
            >= 1300 => 30,
            >= 1100 => 27,
            >= 900 => 24,
            _ => 20
        };

        if (width < 1200)
        {
            rows -= 2;
        }

        if (width < 900)
        {
            rows -= 2;
        }

        return Math.Clamp(rows, MinPageSize, MaxPageSize);
    }

    private async Task OnTabChanged(int tabIndex)
    {
        _activeTabIndex = tabIndex;
        _selectedFilter = tabIndex switch
        {
            0 => LeaderboardFilter.ThisWeek,
            1 => LeaderboardFilter.ThisMonth,
            2 => LeaderboardFilter.ThisYear,
            3 => LeaderboardFilter.Forever,
            _ => LeaderboardFilter.ThisWeek
        };

        _currentPage = 0;
        _scrollProgress = 0;
        _totalPages = 1;

        // Reset scroll timer so full 10s from now
        _scrollTimer?.Stop();
        _scrollTimer?.Start();

        await LoadLeaderboard();
    }

    private string GetTimeAgoText()
    {
        if (_lastUpdated is null) return "never";

        var elapsed = DateTime.Now - _lastUpdated.Value;
        if (elapsed.TotalSeconds < 30) return "just now";
        if (elapsed.TotalMinutes < 1.5) return "1 minute ago";
        return $"{(int)elapsed.TotalMinutes} minutes ago";
    }

    public void Dispose()
    {
        _refreshTimer?.Stop();
        _refreshTimer?.Dispose();
        _countdownTimer?.Stop();
        _countdownTimer?.Dispose();
        _scrollTimer?.Stop();
        _scrollTimer?.Dispose();
        _progressTimer?.Stop();
        _progressTimer?.Dispose();
    }

    private class ViewportSize
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
