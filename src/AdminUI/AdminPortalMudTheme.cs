using MudBlazor;

namespace SSW.Rewards.Admin.UI;

public static class AdminPortalMudTheme
{
    static AdminPortalMudTheme()
    {
        Theme = new MudTheme()
        {
            Palette = new PaletteLight()
            {
                Primary = "#cc4141",
                Secondary = "#333333",
                AppbarBackground = "#cc4141",
                Background = "#ffffffff",
            },
            PaletteDark = new PaletteDark()
            {
                Primary = "#cc4141",
                Black = "#27272f",
                Background = "#181818",
                BackgroundGrey = "#27272f",
                Surface = "#373740",
                DrawerBackground = "#333333",
                DrawerText = "#ffffffff",
                DrawerIcon = "rgba(255,255,255, 0.50)",
                AppbarBackground = "#cc4141",
                AppbarText = "rgba(255,255,255, 0.70)",
                TextPrimary = "#ffffffff",
                TextSecondary = "rgba(255,255,255, 0.50)",
                ActionDefault = "#adadb1",
                ActionDisabled = "rgba(255,255,255, 0.26)",
                ActionDisabledBackground = "rgba(255,255,255, 0.12)",
                Divider = "rgba(255,255,255, 0.12)",
                DividerLight = "rgba(255,255,255, 0.06)",
                TableLines = "rgba(255,255,255, 0.12)",
                LinesDefault = "rgba(255,255,255, 0.12)",
                LinesInputs = "rgba(255,255,255, 0.3)",
                TextDisabled = "rgba(255,255,255, 0.2)",
                Info = "#3299ff",
                Success = "#0bba83",
                Warning = "#ffa800",
                Error = "#f64e62",
                Dark = "#27272f",
            },
        };
    }

    public static MudTheme Theme { get; set; }
}