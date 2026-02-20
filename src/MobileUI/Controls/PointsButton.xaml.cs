using CommunityToolkit.Maui;

namespace SSW.Rewards.Mobile.Controls;

public partial class PointsButton : ContentView
{
    [BindableProperty]
    public partial int Points { get; set; }
    
    [BindableProperty]
    public partial string ButtonText { get; set; }

    [BindableProperty]
    public partial bool IsDisabled { get; set; }
    
    public PointsButton()
    {
        InitializeComponent();
    }
}