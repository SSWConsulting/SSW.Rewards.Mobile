# .NET MAUI Patterns

## MVVM with CommunityToolkit.Mvvm — partial properties

Use `[ObservableProperty]` on **partial properties** (CommunityToolkit.Mvvm 8.4+, C# 13+), not on
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

## Offline-ready list pages — `AdvancedObservableCollection<T>`

**Every list page must render from cache when offline.** The pattern (exemplar:
`Features/Activity/ActivityPageViewModel.cs`; also `Features/Network/NetworkPageViewModel.cs`):

```csharp
public AdvancedObservableCollection<ActivityFeedItemDto> Feed { get; } = new();

public MyViewModel(IFileCacheService fileCacheService, ...)
{
    // 1. Cache the initial load (first page); pagination/secondary segments stay network-only.
    Feed.InitializeInitialCaching(fileCacheService, "MyPageCache", () => _skip == 0);

    // 2. Anti-flicker: skip the UI replace when cache and network data are identical.
    Feed.CompareItems = AreSameItem;

    // 3. Per-item display fields (relative timestamps, fallbacks) — runs for BOTH cache and
    //    network results, so cached items are re-enriched fresh.
    Feed.OnDataReceived += OnDataReceived;

    // 4. Track state; `isFromCache` here is the truth for "showing cached data".
    Feed.OnCollectionUpdated += (items, isFromCache) => { IsShowingCachedData = isFromCache; IsRefreshing = false; };

    // 5. MANDATORY. Without OnError, a failed background refresh RETHROWS and crashes the app.
    Feed.OnError += OnError;
}

public Task LoadFeed() => Feed.LoadAsync(FetchPage, reload: true);   // initial/refresh: replace
// pagination: Feed.LoadAsync(FetchPage) — append, no cache
```

Rules:

1. **`OnError` is mandatory** — wire it before shipping any page that uses the collection.
   Alert only when nothing is on screen (`Feed.Collection.Count == 0`); a failed background
   refresh behind cached data stays silent. Route through `ExceptionHandler.HandleApiException`
   first (401 → login).
2. **Initial load and pull-to-refresh use `reload: true`** — the cache-then-network double
   callback otherwise appends the first page twice.
3. **Pagination is network-only** (`shouldUseCache` returns false past page 1) and only a
   **successful empty page** may set the "limit reached" flag — an offline failure must not
   latch it (track the last network fetch count via `OnDataReceived`'s `!isFromCache` branch).
4. **XAML binds the inner collection**: `ItemsSource="{Binding Feed.Collection}"`.
5. Online-only actions (submit, redeem) check `Connectivity.Current.NetworkAccess` and tell the
   user why they're blocked — never let the action throw.

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
4. List pages **must** work offline via `AdvancedObservableCollection<T>` (see above)
5. Navigation uses Shell-based navigation
6. DI uses built-in .NET DI container

## Key Packages

- `CommunityToolkit.Maui` & `CommunityToolkit.Mvvm`
- `FFImageLoading.Maui` (image caching)
- `BarcodeScanning.Native.Maui` (QR scanning)
- `Mopups` (modal popups)
- `Plugin.Firebase.*` (Analytics, Crashlytics, Messaging)
