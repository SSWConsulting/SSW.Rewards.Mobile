using System.Windows.Input;
using Maui.BindableProperty.Generator.Core;

namespace SSW.Rewards.Mobile.Controls;

public partial class CustomTabBar : TabBar
{
    public event EventHandler CenterViewTapped;
    [AutoBindable]
    private ImageSource? centerViewImageSource;
    [AutoBindable]
    private string? centerViewText;
    [AutoBindable]
    private bool centerViewVisible;
    [AutoBindable]
    public Color? centerViewBackgroundColor;
    
    public void CenterView_Tapped()
    {
        CenterViewTapped?.Invoke(this, EventArgs.Empty);
    }
}