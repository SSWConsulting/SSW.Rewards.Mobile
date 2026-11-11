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
    IApiErrorPresenter errorPresenter, ...)
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
    await _errorPresenter.HandleLoadAsync(result, userRequestedNewData: false, "the activity feed");
}

[RelayCommand]
private async Task LoadMore()
{
    var result = await Feed.LoadMoreAsync();   // never discard the result
    await _errorPresenter.HandleLoadAsync(result, userRequestedNewData: false, "the activity feed");
}

[RelayCommand(AllowConcurrentExecutions = true)]  // a non-concurrent command drops the user's next tap
private async Task FilterBySegment()
{
    var segment = (MySegments)SelectedSegment.Value;
    _currentSegment = segment;
    var result = await Feed.RefreshAsync(queryChanged: true);
    if (segment != _currentSegment) return;          // the user already moved on
    await _errorPresenter.HandleLoadAsync(result, userRequestedNewData: true, "the activity feed");
}
```

```xml
<CollectionView ItemsSource="{Binding Feed.Items}" ... />
```

The cache behind `IFileCacheService` is **Akavache v12** (SQLite `LocalMachine` store) — pass an
`expiry` to `SetAsync` for data that goes stale; reads/writes never throw. The pattern stays
cache-implementation-agnostic: pages only ever see `IFileCacheService`.

Rules:

1. **Report every failure via `IApiErrorPresenter`** (Core, DI singleton, unit-tested) — list loads,
   other loads and user actions alike; don't write alert text or hand-roll the policy. It runs the API
   exception handler first (401 → login), stays silent when a background load fails behind visible
   content, and otherwise says "You're offline" or "Something went wrong" with wording from
   `UserMessages`. You pass only the subject ("the activity feed") or action ("redeem a reward").
   Pass `userRequestedNewData: true` for segment/filter switches (those always alert). A legacy
   `OnError(Exception)` callback reports through `HandleLoadAsync(ListLoadResult.Fail(ex, hasContent), ...)`.
2. **API client methods throw `HttpRequestException` with `StatusCode`** on failure — a plain
   `Exception` hides 401s from the handler, so an expired session behind cached content never logs in.
   The resilience pipeline's own failures (timeout, open circuit) also arrive as `HttpRequestException`,
   and nothing is retried while `IApiNetworkStatus` says the device is offline, so offline errors are immediate.
3. **No MAUI statics in ViewModels.** Use the Core seams: `IConnectivityService` (not
   `Connectivity.Current`), `IAppNavigator` (not `Shell.Current` + `ActivatorUtilities`),
   `IApiErrorPresenter`/`IApiExceptionHandler`, `IAlertService`. This is what keeps
   ViewModel logic host-testable.
4. **`RefreshAsync` replaces, `LoadMoreAsync` appends.** Pagination is network-only and can't
   duplicate page 1 while cached data is shown; only a successful short/empty page marks the end.
   A failed refresh keeps the shown items *and* their paging position — pass `queryChanged: true`
   when `FetchPage` now returns a different list (segment/filter): the new query's cache is shown if it
   has one, and a failure clears the old query's items rather than showing them under the new one.
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
9. **User data is session-scoped.** Cache it only through `IFileCacheService` (reset on logout and on
   a 401), and drop any in-flight result that outlives the session (see `UserService`'s session token).
   Writing outside `PagedListSource`/`FetchAndRefresh`? Capture `Generation` before the load and pass it
   to `SetAsync`, so a load that outlives a reset can't write the previous user's data back.
10. `PagedListSource` is bindable — `Feed.Items` in XAML, `Feed.IsShowingCachedData` for
   stale-data indicators (global offline banner lands with #1567).
11. **Writes are online-only** (no outbox; scans included). Guard each one with
   `if (!await _errorPresenter.EnsureOnlineAsync("redeem a reward")) return;` and report a failed
   request with `HandleActionErrorAsync(ex, action)` — never let the action throw.
12. **Detail pages** (one item, not a list) use `CachedItemLoader.LoadAsync` with a per-item key
   (`PostDetail_{id}`): cached copy first, then fresh, same `ListLoadResult` for the error handler.
   Reloads after a write skip the cached copy (it predates the change) — see `PostDetailViewModel`.
   APIs that page by page number adapt with `PageNumberPaging.ToPageIndex(skip, take)`.
13. `AdvancedObservableCollection<T>` (Network/Leaderboard/Redeem/Profile) is the **legacy**
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
