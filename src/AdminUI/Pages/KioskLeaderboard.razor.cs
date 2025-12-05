using Microsoft.AspNetCore.Components;
using MudBlazor;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Enums;
using SSW.Rewards.Shared.DTOs.Leaderboard;

namespace SSW.Rewards.Admin.UI.Pages;

public partial class KioskLeaderboard : IDisposable
{
    private const int RefreshIntervalSeconds = 60;
    private const int ScrollIntervalSeconds = 10;

    [Inject] private ILeaderboardService leaderboardService { get; set; } = default!;

    private MudTable<MobileLeaderboardUserDto> table = default!;
    private LeaderboardFilter _selectedFilter = LeaderboardFilter.ThisWeek;
    private int _activeTabIndex = 0;
    private readonly int[] _pageSizeOptions = [30, 50, 100];
    private int _defaultPageSize = 30;
    private System.Timers.Timer? _refreshTimer;
    private System.Timers.Timer? _countdownTimer;
    private System.Timers.Timer? _scrollTimer;
    private System.Timers.Timer? _progressTimer;
    private DateTime? _lastUpdated;
    private bool _hasErrorsOnUpdate = false;
    private int _secondsUntilRefresh = RefreshIntervalSeconds;
    private int _currentPage = 0;
    private int _totalPages = 0;
    private double _progressBarWidth = 0;
    private DateTime _lastPageChange = DateTime.Now;

    private TableData<MobileLeaderboardUserDto> _lastTableCache = new() { TotalItems = 0, Items = [] };

    protected override async Task OnInitializedAsync()
    {
        await LoadLeaderboard();

        // Refresh data (default every 60 seconds)
        _refreshTimer = new System.Timers.Timer(RefreshIntervalSeconds * 1000);
        _refreshTimer.Elapsed += async (_, __) => await InvokeAsync(async () =>
        {
            _secondsUntilRefresh = RefreshIntervalSeconds;
            await LoadLeaderboard();
        });
        _refreshTimer.AutoReset = true;
        _refreshTimer.Start();

        // Update countdown every second
        _countdownTimer = new System.Timers.Timer(1000); // 1 second
        _countdownTimer.Elapsed += async (_, __) => await InvokeAsync(() =>
        {
            if (_secondsUntilRefresh > 0)
                _secondsUntilRefresh--;
            StateHasChanged();
        });
        _countdownTimer.AutoReset = true;
        _countdownTimer.Start();

        // Auto-scroll through pages (default every 10 seconds)
        _scrollTimer = new System.Timers.Timer(ScrollIntervalSeconds * 1000);
        _scrollTimer.Elapsed += async (_, __) => await InvokeAsync(() =>
        {
            // Only scroll if there's more than one page and we didn't just refresh.
            if (_totalPages > 1 && RefreshIntervalSeconds - _secondsUntilRefresh >= ScrollIntervalSeconds)
            {
                _currentPage = (_currentPage + 1) % _totalPages;
                _lastPageChange = DateTime.Now; // Reset the timer for progress animation
                if (table != null)
                {
                    table.NavigateTo(_currentPage);
                }
            }
        });
        _scrollTimer.AutoReset = true;
        _scrollTimer.Start();

        // Update progress bar animation (every 100ms for smooth animation)
        _progressTimer = new System.Timers.Timer(100);
        _progressTimer.Elapsed += async (_, __) => await InvokeAsync(() =>
        {
            UpdateProgressBar();
        });
        _progressTimer.AutoReset = true;
        _progressTimer.Start();
    }

    private void UpdateProgressBar()
    {
        if (_totalPages <= 1)
        {
            _progressBarWidth = 100; // Full width if only one page
            StateHasChanged();
            return;
        }

        // Calculate time elapsed since last page change
        var elapsedSeconds = (DateTime.Now - _lastPageChange).TotalSeconds;
        
        // Calculate progress within current page (0-100% over ScrollIntervalSeconds)
        var pageProgress = Math.Min(100, (elapsedSeconds / (double)ScrollIntervalSeconds) * 100.0);
        
        // Calculate base progress from completed pages
        var baseProgress = ((double)_currentPage / _totalPages) * 100;
        
        // Calculate progress increment for current page
        var pageIncrement = (100.0 / _totalPages) * (pageProgress / 100);
        
        _progressBarWidth = 100 - pageProgress;
        
        StateHasChanged();
    }

    private async Task<TableData<MobileLeaderboardUserDto>> ServerReload(TableState state)
    {
        try
        {
            var result = await leaderboardService.GetMobilePaginatedLeaderboard(state.Page, _defaultPageSize, _selectedFilter, CancellationToken.None);
            _lastUpdated = DateTime.Now;
            _hasErrorsOnUpdate = false;

            // Calculate total pages based on users with points only
            var usersWithPointsOnPage = result.Items.Count(u => u.Points > 0);

            // If this page has users with 0 points or fewer users than page size, we've found the boundary
            if (usersWithPointsOnPage < _defaultPageSize || usersWithPointsOnPage < result.Items.Count())
            {
                // Calculate exact total: all previous pages (full) + users with points on this page
                var totalUsersWithPoints = (state.Page * _defaultPageSize) + usersWithPointsOnPage;
                _totalPages = Math.Max(1, (int)Math.Ceiling((double)totalUsersWithPoints / _defaultPageSize));
            }
            else if (usersWithPointsOnPage == _defaultPageSize && result.Items.Count() == _defaultPageSize)
            {
                // This page is full of users with points, there might be more pages
                // We'll know the exact count when we hit a page with 0-point users or incomplete page
                // For now, assume at least one more page exists
                _totalPages = state.Page + 2; // Current page + at least one more
            }
            else
            {
                // Fallback: at least the current page
                _totalPages = Math.Max(1, state.Page + 1);
            }

            _lastTableCache = new() { TotalItems = result.Count, Items = result.Items };
        }
        catch (Exception ex)
        {
            _hasErrorsOnUpdate = true;
            Console.WriteLine($"Error in ServerReload: {ex.Message}");
        }
        
        // Reset progress bar when data reloads
        _lastPageChange = DateTime.Now;
        StateHasChanged();
        return _lastTableCache;
    }

    private async Task LoadLeaderboard()
    {
        if (table != null)
        {
            if (_currentPage != 0)
            {
                _currentPage = 0;
                table.NavigateTo(_currentPage);
            }

            await table.ReloadServerData();
        }
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

        await LoadLeaderboard();
    }

    private string GetLastUpdatedText()
        => _lastUpdated?.ToString("dd MMMM yyyy HH:mm")
            ?? "never";

    public void Dispose()
    {
        if (_refreshTimer != null)
        {
            _refreshTimer.Stop();
            _refreshTimer.Dispose();
            _refreshTimer = null;
        }

        if (_countdownTimer != null)
        {
            _countdownTimer.Stop();
            _countdownTimer.Dispose();
            _countdownTimer = null;
        }

        if (_scrollTimer != null)
        {
            _scrollTimer.Stop();
            _scrollTimer.Dispose();
            _scrollTimer = null;
        }

        if (_progressTimer != null)
        {
            _progressTimer.Stop();
            _progressTimer.Dispose();
            _progressTimer = null;
        }
    }
}
