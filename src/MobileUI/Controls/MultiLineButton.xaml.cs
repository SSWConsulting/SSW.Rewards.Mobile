using System.Windows.Input;
using CommunityToolkit.Maui;

namespace SSW.Rewards.Mobile.Controls;

public partial class MultiLineButton
{
    [BindableProperty]
    public partial string Text { get; set; }

    [BindableProperty]
    public partial Color TextColor { get; set; } = Colors.White;

    [BindableProperty]
    public partial int FontSize { get; set; } = 14;

    [BindableProperty]
    public new partial Color BackgroundColor { get; set; } = App.Current.Resources["SSWRed"] as Color;

    [BindableProperty]
    public partial ICommand Command { get; set; }

    public MultiLineButton()
    {
        InitializeComponent();
    }
}