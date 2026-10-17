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
(`MobileUI.Core/Paging/PagedListSource.cs`) owns the whole load lifecycle so the ViewModel only declares intent:
cache-then-network initial load, pull-to-refresh, network-only pagination with end-of-list
detection, per-item display preparation, anti-flicker replace, and stale-load cancellation.

Exemplar: `Features/Activity/ActivityPageViewModel.cs`.

```csharp
public PagedListSource<ActivityFeedItemDto> Feed { get; }

public MyViewModel(IActivityFeedService service, IFileCacheService fileCacheService,
    OfflineAwareListErrorHandler errorHandler, ...)
{
    Feed = new PagedListSource<ActivityFeedItemDto>(new()
    {
        FetchPage = async (skip, take, ct) => (await service.GetAllActivities(take, skip, ct)).Feed.ToList(),
        PrepareItem = item => item.PrepareForDisplay(),        // display fields, cached AND fresh
        AreSame = (a, b) => a.HasSameDisplayAs(b),             // EVERY rendered field, not just the key
        Cache = fileCacheService,
        CacheKey = "MyPageCache",
        ShouldUseCache = () => _currentSegment == MySegments.Default,  // optional extra gate
    });
}

public async Task LoadFeed()
{
    var result = await Feed.RefreshAsync();
    // 401 → login even behind cache; other failures stay quiet while content is shown.
    await _errorHandler.HandleAsync(result, userRequestedNewData: false, Title, OfflineMessage, GenericMessage);
}

[RelayCommand]
private async Task LoadMore()
{
    var result = await Feed.LoadMoreAsync();   // never discard the result
    await _errorHandler.HandleAsync(result, userRequestedNewData: false, Title, OfflineMessage, GenericMessage);
}

[RelayCommand(AllowConcurrentExecutions = true)]  // a non-concurrent command drops the user's next tap
private async Task FilterBySegment()
{
    var segment = (MySegments)SelectedSegment.Value;
    _currentSegment = segment;
    var result = await Feed.RefreshAsync(queryChanged: true);
    if (segment != _currentSegment) return;          // the user already moved on
    await _errorHandler.HandleAsync(result, userRequestedNewData: true, Title, OfflineMessage, GenericMessage);
}
```

```xml
<CollectionView ItemsSource="{Binding Feed.Items}" ... />
```

The cache behind `IFileCacheService` is **Akavache v12** (SQLite `LocalMachine` store) — pass an
`expiry` to `SetAsync` for data that goes stale; reads/writes never throw. The pattern stays
cache-implementation-agnostic: pages only ever see `IFileCacheService`.

Rules:

1. **Handle every result via `OfflineAwareListErrorHandler`** (Core, DI-registered, unit-tested) —
   refresh, load-more and segment switches alike; don't hand-roll the policy. It runs the API
   exception handler first (401 → login), stays silent when a background load fails behind visible
   content, and otherwise shows your offline/generic message. Pass `userRequestedNewData: true` for
   segment/filter switches (those always alert).
2. **API client methods throw `HttpRequestException` with `StatusCode`** on failure — a plain
   `Exception` hides 401s from the handler, so an expired session behind cached content never logs in.
3. **No MAUI statics in ViewModels.** Use the Core seams: `IConnectivityService` (not
   `Connectivity.Current`), `IAppNavigator` (not `Shell.Current` + `ActivatorUtilities`),
   `IApiExceptionHandler` (not the static `ExceptionHandler`), `IAlertService`. This is what keeps
   ViewModel logic host-testable.
4. **`RefreshAsync` replaces, `LoadMoreAsync` appends.** Pagination is network-only and can't
   duplicate page 1 while cached data is shown; only a successful short/empty page marks the end.
   A failed refresh keeps the shown items *and* their paging position — pass `queryChanged: true`
   when `FetchPage` now returns a different list (segment/filter), so a failure can't page the old one.
5. **Loads supersede each other.** A newer refresh cancels older loads, whose results (successes
   *and* failures) come back as no-error; a `LoadMore` requested mid-refresh runs once it settles.
6. **`AreSame` must compare every displayed field.** When all items match, the shown instances are
   kept and the fresh ones discarded — compare only the key and renames/avatars/relative times stick.
7. **`PrepareItem` is for display-only fields** (relative timestamps, avatar fallbacks) — it runs
   for cached and fresh items alike, so never put it in the fetch path yourself.
8. **Transient ViewModels don't subscribe to singleton observables** (`IUserService.My*Observable`) —
   the singleton keeps them alive, and page-pop hooks don't run when auth replaces the root page. Read
   the current value when needed (`await observable.FirstAsync()`); if a page must react live,
   subscribe in `OnAppearing` and dispose in `OnDisappearing`.
9. **User data is session-scoped.** Cache it only through `IFileCacheService` (reset on logout), and
   drop any in-flight result that outlives the session (see `UserService`'s session token).
10. `PagedListSource` is bindable — `Feed.Items` in XAML, `Feed.IsShowingCachedData` for
   stale-data indicators (global offline banner lands with #1567).
11. Online-only actions (submit, redeem) check `Connectivity.Current.NetworkAccess` and tell the
   user why they're blocked — never let the action throw.
12. `AdvancedObservableCollection<T>` (Network/Leaderboard/Redeem/Profile) is the **legacy**
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
