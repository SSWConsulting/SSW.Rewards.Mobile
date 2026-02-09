# Blazor WASM Patterns

## Component Pattern

```csharp
@inject IUserService UserService

<MudDataGrid Items="@_users" Loading="@_loading">
    <Columns>
        <PropertyColumn Property="x => x.Name" />
        <PropertyColumn Property="x => x.Email" />
    </Columns>
</MudDataGrid>

@code {
    private List<UserDto> _users = [];
    private bool _loading;

    protected override async Task OnInitializedAsync()
    {
        _loading = true;
        _users = await UserService.GetUsersAsync();
        _loading = false;
    }
}
```

## Key Rules

1. Use **MudBlazor** components for UI
2. Use scoped services for state per session
3. Use custom API client with auth handler for HTTP

## UI Library

- **MudBlazor 6.x** - Material Design components
- Authentication via `Microsoft.AspNetCore.Components.WebAssembly.Authentication`

## Styling

**Read [adminui-styling.md](adminui-styling.md) BEFORE making CSS changes.** Key rules:

1. **`::deep` is required** for styling MudBlazor components and child components in scoped CSS
2. Use the color palette from `AdminPortalMudTheme.cs` — don't introduce new hex colors
3. Prefer MudBlazor parameters (`Color`, `Variant`, `Size`) over CSS overrides
4. Use `/style-adminui` Claude skill when working on AdminUI styling tasks
