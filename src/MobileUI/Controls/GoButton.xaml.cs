using Maui.BindableProperty.Generator.Core;

namespace SSW.Rewards.Mobile.Controls;

public partial class GoButton : ContentView
{
    [AutoBindable]
    private int _points;
    
	public GoButton()
	{
		InitializeComponent();
	}
}