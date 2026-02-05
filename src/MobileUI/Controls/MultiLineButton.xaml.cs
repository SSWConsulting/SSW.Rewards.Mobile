using System.Windows.Input;
using CommunityToolkit.Maui;

namespace SSW.Rewards.Mobile.Controls;

public partial class MultiLineButton
{
    private static readonly Lazy<Color> DefaultBackgroundColor = new(() =>
        App.Current?.Resources.TryGetValue("SSWRed", out var color) == true && color is Color c
            ? c
            : Colors.Red); // Fallback color if resource not found

    [BindableProperty]
    public partial string Text { get; set; }

    [BindableProperty]
    public partial Color TextColor { get; set; } = Colors.White;

    [BindableProperty]
    public partial int FontSize { get; set; } = 14;

    [BindableProperty]
    public new partial Color BackgroundColor { get; set; }

    [BindableProperty]
    public partial ICommand Command { get; set; }

    public MultiLineButton()
    {
        InitializeComponent();
        
        // Set default background color after initialization
        BackgroundColor ??= DefaultBackgroundColor.Value;
    }
}