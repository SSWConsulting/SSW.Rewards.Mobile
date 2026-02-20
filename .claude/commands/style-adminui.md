# AdminUI Styling Assistant

You are helping with CSS/styling changes in the AdminUI Blazor WASM project.

**BEFORE writing any CSS**, read these files:
1. `_docs/agents/adminui-styling.md` — Complete styling guide with rules and patterns
2. `src/AdminUI/AdminPortalMudTheme.cs` — MudBlazor theme (colors, typography)
3. `src/AdminUI/wwwroot/css/app.css` — Global styles and CSS variables
4. The specific `.razor` and `.razor.css` files you're modifying

## Critical: Blazor CSS Isolation

**The #1 cause of "my CSS isn't working" bugs:**

Blazor scoped CSS (`.razor.css`) ONLY applies to elements rendered directly by that component. Elements inside **child components** (including MudBlazor components like `MudTable`, `MudTabs`, `MudButton`) need `::deep` to be styled.

```css
/* WRONG - will be silently ignored */
.my-table .mud-table-cell { font-size: 1rem; }

/* CORRECT - pierces into MudBlazor component */
::deep .my-table .mud-table-cell { font-size: 1rem; }
```

**Rule**: If you're styling anything with a `mud-` class prefix, you MUST use `::deep`.

## Color Palette (Do Not Introduce New Colors)

| Color | Hex | Use For |
|-------|-----|---------|
| SSW Red | `#cc4141` | Primary actions, active states, brand |
| Background | `#181818` | Page backgrounds |
| Surface | `#333333` | Cards, drawers, elevated elements |
| Alt Surface | `#222` | Table alt rows, secondary backgrounds |
| Header | `#525252` | Table headers |
| Text | `#ffffff` | Primary text |
| Text Muted | `rgba(255,255,255,0.5)` | Secondary text |
| Caption | `#aaa` | Captions, metadata |
| Error | `#ff6161` | Error states |

## Checklist Before Writing CSS

1. Does the target element live inside a MudBlazor or child component? → Use `::deep`
2. Am I using a color that's already in the palette? → Reuse, don't create new shades
3. Can I achieve this with MudBlazor parameters instead of CSS? → Prefer parameters
4. Does this need `!important`? → Only for MudBlazor overrides, add a comment
5. Is the content responsive? → Test at 375px, 768px, 1200px+
