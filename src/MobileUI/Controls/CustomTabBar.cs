using CommunityToolkit.Maui;

namespace SSW.Rewards.Mobile.Controls;

public partial class CustomTabBar : TabBar
{
    public event EventHandler CenterViewTapped;
    
    [BindableProperty]
    public partial ImageSource CenterViewImageSource { get; set; }
    
    [BindableProperty]
    public partial string CenterViewText { get; set; }
    
    [BindableProperty]
    public partial bool CenterViewVisible { get; set; }
    
    [BindableProperty]
    public partial Color CenterViewBackgroundColor { get; set; }
    
    public void CenterView_Tapped()
    {
        CenterViewTapped?.Invoke(this, EventArgs.Empty);
    }
}