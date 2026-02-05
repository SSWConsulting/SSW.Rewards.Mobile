namespace SSW.Rewards.Mobile.Controls;

public partial class PointsButton
{
    [CommunityToolkit.Maui.BindableProperty]
    public partial int Points { get; set; }
    
    [CommunityToolkit.Maui.BindableProperty]
    public partial string ButtonText { get; set; }

    [CommunityToolkit.Maui.BindableProperty]
    public partial bool IsDisabled { get; set; }
    
    public PointsButton()
    {
        InitializeComponent();
    }
}