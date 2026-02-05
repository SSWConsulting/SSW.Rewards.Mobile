using System.Windows.Input;

namespace SSW.Rewards.Mobile.Controls;

public partial class MultiLineButton
{
    [CommunityToolkit.Maui.BindableProperty]
    public partial string Text { get; set; }

    [CommunityToolkit.Maui.BindableProperty]
    public partial Color TextColor { get; set; } = Colors.White;

    [CommunityToolkit.Maui.BindableProperty]
    public partial int FontSize { get; set; } = 14;
    
    [CommunityToolkit.Maui.BindableProperty]
    public new partial Color BackgroundColor { get; set; } = App.Current.Resources["SSWRed"] as Color;
    
    [CommunityToolkit.Maui.BindableProperty]
    public partial ICommand Command { get; set; }
    
    public MultiLineButton()
    {
        InitializeComponent();
    }
}