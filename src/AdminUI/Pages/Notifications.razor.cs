using MudBlazor;
using SSW.Rewards.Shared.DTOs.Notifications;

namespace SSW.Rewards.Admin.UI.Pages;

public partial class Notifications
{
    private bool _loading;
    private string _searchString = string.Empty;
    private bool _showArchived;
    private MudTable<NotificationHistoryDto> _table;

    protected override async Task OnInitializedAsync()
    {
        await LoadNotificationsFromSearch();
    }

    private async Task<TableData<NotificationHistoryDto>> LoadNotifications(TableState state)
    {
        _loading = true;
        try
        {
            var page = state.Page;
            var pageSize = state.PageSize > 0 ? state.PageSize : 20;
            var sortLabel = state.SortLabel;
            var sortDirection = state.SortDirection == SortDirection.None ? null : state.SortDirection.ToString().ToLower();
            var result = await NotificationsService.GetNotificationHistoryListAsync(page, pageSize, _searchString, sortLabel, sortDirection, _showArchived, CancellationToken.None);
            return new TableData<NotificationHistoryDto>
            {
                Items = result.Items.ToList(),
                TotalItems = result.Count
            };
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task LoadNotificationsFromSearch()
    {
        if (_table != null)
        {
            await _table.ReloadServerData();
        }
    }

    private async Task DeleteNotification(int id)
    {
        _loading = true;
        try
        {
            await NotificationsService.DeleteNotificationAsync(id, CancellationToken.None);
            await LoadNotificationsFromSearch();
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task OnShowArchivedChanged(bool value)
    {
        _showArchived = value;
        await LoadNotificationsFromSearch();
    }
}