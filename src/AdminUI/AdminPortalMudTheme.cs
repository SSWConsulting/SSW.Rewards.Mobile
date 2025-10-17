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
            Typography = new Typography()
            {
                Default = new Default
                {
                    FontSize = "1rem", // Increased from default (0.875rem)
                },
                Body1 = new Body1
                {
                    FontSize = "1.125rem", // Increased from default (1rem)
                },
                Body2 = new Body2
                {
                    FontSize = "1rem", // Increased from default (0.875rem)
                },
                H1 = new H1
                {
                    FontSize = "6.5rem", // Increased from default (6rem)
                },
                H2 = new H2
                {
                    FontSize = "4rem", // Increased from default (3.75rem)
                },
                H3 = new H3
                {
                    FontSize = "3.25rem", // Increased from default (3rem)
                },
                H4 = new H4
                {
                    FontSize = "2.375rem", // Increased from default (2.125rem)
                },
                H5 = new H5
                {
                    FontSize = "1.625rem", // Increased from default (1.5rem)
                },
                H6 = new H6
                {
                    FontSize = "1.375rem", // Increased from default (1.25rem)
                },
                Subtitle1 = new Subtitle1
                {
                    FontSize = "1.125rem", // Increased from default (1rem)
                },
                Subtitle2 = new Subtitle2
                {
                    FontSize = "1rem", // Increased from default (0.875rem)
                },
                Button = new Button
                {
                    FontSize = "1rem", // Increased from default (0.875rem)
                },
                Caption = new Caption
                {
                    FontSize = "0.875rem", // Increased from default (0.75rem)
                }
            }
        };
    }

    public static MudTheme Theme { get; set; }
}