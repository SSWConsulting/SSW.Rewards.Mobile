# .NET MAUI Patterns

## MVVM with CommunityToolkit.Mvvm — partial properties

Use `[ObservableProperty]` on **partial properties** (CommunityToolkit.Mvvm 8.4.2+, C# 13+), not on
private fields. Same generated `INotifyPropertyChanged` plumbing, but no `_field` vs `Property`
duality, natural nullability annotations, and "find all references" works.

```csharp
public partial class HomeViewModel : BaseViewModel
{
    [ObservableProperty]
    public partial string Username { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [RelayCommand]
    private async Task LoadDataAsync() { /* ... */ }
}
```

- Partial properties can't have initializers — set defaults in the constructor.
- Do NOT add Fody/PropertyChanged.Fody — IL weaving fights MAUI trimming/AOT and was superseded
  by these source generators.
- Legacy field-based `[ObservableProperty] private bool _isLoading;` still exists in older
  ViewModels — migrate opportunistically when touching a file; generated property names don't
  change, so XAML is unaffected.

## Offline-ready list pages — `PagedListSource<T>`

**Every list page must render from cache when offline.** `PagedListSource<T>`
(`Common/PagedListSource.cs`) owns the whole load lifecycle so the ViewModel only declares intent:
cache-then-network initial load, pull-to-refresh, network-only pagination with end-of-list
detection, per-item display preparation, anti-flicker replace, and stale-load cancellation.

Exemplar: `Features/Activity/ActivityPageViewModel.cs`.

```csharp
public PagedListSource<ActivityFeedItemDto> Feed { get; }

public MyViewModel(IActivityFeedService service, IFileCacheService fileCacheService, ...)
{
    Feed = new PagedListSource<ActivityFeedItemDto>(new()
    {
        FetchPage = async (skip, take, ct) => (await service.GetAllActivities(take, skip, ct)).Feed.ToList(),
        PrepareItem = item => item.PrepareForDisplay(),          // display fields, cached AND fresh
        AreSame = (a, b) => a.UserId == b.UserId && a.AwardedAt == b.AwardedAt,  // anti-flicker
        Cache = fileCacheService,
        CacheKey = "MyPageCache",
        ShouldUseCache = () => _currentSegment == MySegments.Default,  // optional extra gate
    });
}

public async Task LoadFeed()
{
    var result = await Feed.RefreshAsync();
    if (result.Error is not null && !result.HasContent)     // quiet behind cached content
    {
        await ShowLoadFailedAlert(result.Error);            // ExceptionHandler first, then offline-aware message
    }
}

[RelayCommand] private Task LoadMore() => Feed.LoadMoreAsync();
```

```xml
<CollectionView ItemsSource="{Binding Feed.Items}" ... />
```

The cache behind `IFileCacheService` is **Akavache v11** (SQLite `LocalMachine` store) — pass an
`expiry` to `SetAsync` for data that goes stale; reads/writes never throw. The pattern stays
cache-implementation-agnostic: pages only ever see `IFileCacheService`.

Rules:

1. **Handle `result.Error`.** Route through `ExceptionHandler.HandleApiException` first (401 →
   login), then alert with an offline-aware message. Alert policy: a failed **initial/pull-to-refresh
   load behind cached content stays silent**; a failed **segment/filter switch always alerts** (the
   visible items aren't what the user asked for).
2. **`RefreshAsync` replaces, `LoadMoreAsync` appends.** Pagination is network-only and can't
   duplicate page 1 while cached data is shown; only a successful short/empty page marks the end.
3. **`PrepareItem` is for display-only fields** (relative timestamps, avatar fallbacks) — it runs
   for cached and fresh items alike, so never put it in the fetch path yourself.
4. `PagedListSource` is bindable — `Feed.Items` in XAML, `Feed.IsShowingCachedData` for
   stale-data indicators (global offline banner lands with #1567).
5. Online-only actions (submit, redeem) check `Connectivity.Current.NetworkAccess` and tell the
   user why they're blocked — never let the action throw.
6. `AdvancedObservableCollection<T>` (Network/Leaderboard/Redeem/Profile) is the **legacy**
   version of this pattern — don't use it for new pages; those pages migrate to
   `PagedListSource`/`IFileCacheService.GetAsync` as part of #1567.

## XAML with Compiled Bindings

```xml
<ContentPage xmlns:vm="clr-namespace:SSW.Rewards.Mobile.ViewModels"
             x:DataType="vm:HomeViewModel">
    <Label Text="{Binding Username}" />
    <Button Text="Load" Command="{Binding LoadDataCommand}" />
</ContentPage>
```

## Key Rules

1. **Always** use `x:DataType` for compiled bindings
2. **Always** use `[ObservableProperty]` (partial-property form) for bindable properties
3. **Always** use `[RelayCommand]` for commands
4. List pages **must** work offline via `PagedListSource<T>` (see above)
5. Navigation uses Shell-based navigation
6. DI uses built-in .NET DI container

## Key Packages

- `CommunityToolkit.Maui` & `CommunityToolkit.Mvvm`
- `FFImageLoading.Maui` (image caching)
- `BarcodeScanning.Native.Maui` (QR scanning)
- `Mopups` (modal popups)
- `Plugin.Firebase.*` (Analytics, Crashlytics, Messaging)
