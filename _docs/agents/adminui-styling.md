# AdminUI Styling Guide (Blazor WASM + MudBlazor)

> **Read this BEFORE making any CSS or layout changes to AdminUI.**

## Tech Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| Framework | Blazor WebAssembly | .NET 10.0 |
| Component Library | MudBlazor | 6.12.0 |
| Layouts | `MainLayout` (admin), `KioskLayout` (kiosk) |
| Theme | `AdminPortalMudTheme.cs` (C# class) |
| Global CSS | `wwwroot/css/app.css` |
| Scoped CSS | `.razor.css` files (Blazor CSS isolation) |
| CSS Variables | `:root` in `app.css` (only 4 defined) |

## Critical Rule: Blazor CSS Isolation + `::deep`

**This is the #1 source of styling bugs in this codebase.**

Blazor CSS isolation works by adding a unique attribute (e.g., `b-abc123`) to elements rendered *directly* by the component. Elements rendered by **child components do NOT get the parent's scope attribute**.

### The Problem

```css
/* KioskLeaderboard.razor.css - SCOPED to KioskLeaderboard */
.kiosk-header-qr {
    width: 100px;  /* SILENTLY IGNORED - element is inside child component */
}
```

```html
<!-- KioskLeaderboard.razor -->
<KioskDownloadPanel Class="kiosk-header-qr" />
<!-- The Class is applied to an element INSIDE KioskDownloadPanel -->
<!-- That element gets KioskDownloadPanel's scope, NOT KioskLeaderboard's -->
```

### The Fix

Use `::deep` to pierce into child component boundaries:

```css
/* KioskLeaderboard.razor.css - NOW WORKS */
::deep .kiosk-header-qr {
    width: 100px;  /* Matches elements in child components */
}
```

### Decision Tree: When to Use `::deep`

```
Is the CSS class applied to an element rendered by THIS component's .razor file?
├── YES → Normal selector (no ::deep needed)
│   Example: <div class="my-class"> in the same .razor file
│
└── NO → Use ::deep
    ├── Element is inside a MudBlazor component (MudTable, MudTabs, etc.)
    │   Example: ::deep .mud-table-cell { ... }
    ├── Element is inside a custom child component
    │   Example: ::deep .kiosk-header-qr { ... }
    └── Element is rendered by a RenderFragment/template
        Example: ::deep .my-template-class { ... }
```

### Common `::deep` Targets in This Codebase

| Pattern | What It Overrides |
|---------|------------------|
| `::deep .mud-table-cell` | MudTable cell styling |
| `::deep .mud-table-row` | MudTable row styling |
| `::deep .mud-tabs-toolbar` | MudTabs toolbar container |
| `::deep .mud-tab` | Individual MudTab button |
| `::deep .mud-tab-active` | Active MudTab styling |
| `::deep .mud-tabs-indicator` | MudTabs underline indicator |
| `::deep .mud-nav-link` | Navigation menu item |
| `::deep .mud-input-*` | Input field styling |

## Color System

### Brand Colors (Use These)

| Name | Hex | Usage |
|------|-----|-------|
| SSW Red | `#cc4141` | Primary brand, buttons, active states |
| Dark Background | `#181818` | Page backgrounds |
| Surface | `#333333` | Cards, drawer, elevated surfaces |
| Dark Surface | `#222` | Table alt rows, secondary surfaces |
| Header Surface | `#525252` | Table headers |
| Text Primary | `#ffffff` | Primary text |
| Text Secondary | `rgba(255,255,255,0.5)` | Secondary/muted text |
| Text Tertiary | `#aaa` or `#9ca3af` | Captions, handles |
| Error | `#ff6161` | Error states |
| Error Light | `#ff9a9a` | Error text on dark backgrounds |

### CSS Variables (defined in `app.css :root`)

```css
--smokey-white: #F7F7F7;     /* Input focus background */
--ssw-red: #cc4141;           /* Brand red */
--validation-pink: #ff8a8a;   /* Validation error text */
--label-grey: rgba(255, 255, 255, 0.5);  /* Form labels */
```

### MudBlazor Theme (defined in `AdminPortalMudTheme.cs`)

Access via MudBlazor components - these are set programmatically:
- `Color.Primary` = `#cc4141`
- `Color.Secondary` = `#333333`
- Dark mode is the DEFAULT theme

### Anti-Pattern: Hard-Coded Colors

Do NOT introduce new hex values when an existing one exists. Common duplicates to avoid:
- `#121212` vs `#181818` - use `#181818` (matches theme `Background`)
- `#222` vs `#27272f` - use the one appropriate for context
- Always use `#cc4141` for SSW Red (not `#CC4141` or other shades)

## Layout Patterns

### Centering Content with Max Width

```html
<!-- Standard pattern for centered, width-constrained content -->
<div style="display: flex; justify-content: center;">
    <div style="width: 600px; max-width: 100%;">
        <!-- Content here -->
    </div>
</div>
```

### MudBlazor Layout Components

```html
<!-- Use MudBlazor's layout system, not raw HTML -->
<MudStack Row="true" Spacing="2" AlignItems="AlignItems.Center">
<MudGrid>
    <MudItem xs="12" md="6">
<MudPaper Class="pa-4" Style="background: #181818;">
```

### Responsive Breakpoints (Used in This Project)

| Breakpoint | Usage |
|-----------|-------|
| `599px` | Mobile threshold |
| `640.98px` | Sidebar collapse |
| `768px` | Tablet |
| `1023px` | iPad portrait |
| `1200px` | Desktop |
| `1400px` | Large desktop |

## Overriding MudBlazor Components

### Strategy (In Order of Preference)

1. **Use MudBlazor parameters** - `Color`, `Variant`, `Size`, `Class`, `Style`
2. **Use the MudTheme** - Modify `AdminPortalMudTheme.cs` for global changes
3. **Use scoped CSS with `::deep`** - For component-specific overrides
4. **Use global CSS in `app.css`** - For cross-cutting overrides (last resort)

### Example: Styling MudTabs

```css
/* In your .razor.css file */
::deep .my-tabs .mud-tabs-toolbar {
    background: #222;
    border-radius: 12px;
}

::deep .my-tabs .mud-tab {
    color: #fff !important;
    background: #222 !important;
    border-radius: 10px !important;
}

::deep .my-tabs .mud-tab.mud-tab-active {
    background: #CC4141 !important;
}

/* Hide the default underline indicator */
::deep .my-tabs .mud-tabs-indicator {
    display: none !important;
}
```

### Example: Styling MudTable

```css
::deep .my-table .mud-table-head .mud-table-cell {
    background-color: #525252 !important;
}

::deep .my-table .mud-table-row:nth-child(2n) {
    background-color: #333;
}

::deep .my-table .mud-table-row:nth-child(2n+1) {
    background-color: #222;
}

::deep .my-table .mud-table-cell {
    font-size: 1rem;
    padding: 0.7rem;
}
```

## QR Code Generation

Uses `QRCoder` library. The `GetGraphic(pixelsPerModule)` parameter controls image size:

```csharp
// pixelsPerModule: each QR module becomes NxN pixels
// QR v3 (29x29 modules) at pixelsPerModule=20 → 580x580px image
// QR v3 (29x29 modules) at pixelsPerModule=5  → 145x145px image
var pngBytes = qrCode.GetGraphic(5); // Use 5 for thumbnails, 10-15 for large displays
```

**Rule**: Match `pixelsPerModule` to the display size. Don't generate a 580px image to display at 100px.

## File Organization

```
src/AdminUI/
├── AdminPortalMudTheme.cs          # MudBlazor theme (colors, typography)
├── wwwroot/css/app.css             # Global styles + CSS variables
├── Shared/
│   ├── MainLayout.razor.css        # Admin layout styles
│   └── KioskLayout.razor.css       # Kiosk layout styles
├── Pages/
│   └── *.razor.css                 # Page-scoped styles
└── Components/
    └── *.razor.css                 # Component-scoped styles
```

## Common Pitfalls Checklist

Before submitting CSS changes, verify:

- [ ] **`::deep` used for child/MudBlazor elements** - Does your CSS target elements inside child components or MudBlazor components? If yes, you MUST use `::deep`.
- [ ] **No new hard-coded colors** - Use existing palette colors from the theme or CSS variables.
- [ ] **`!important` justified** - Only use `!important` when overriding MudBlazor defaults. Add a comment explaining why.
- [ ] **Responsive behavior tested** - Check at mobile (375px), tablet (768px), and desktop (1200px+).
- [ ] **Inline styles minimized** - Prefer scoped CSS classes over `Style="..."` attributes. Inline styles can't use media queries or pseudo-selectors.
- [ ] **Font sizes use theme typography** - Use MudBlazor's `Typo` parameter (e.g., `Typo.body1`) instead of raw font-size values.
