using CommunityToolkit.Mvvm.ComponentModel;
using Maui.BindableProperty.Generator.Core;

namespace SSW.Rewards.Mobile.Controls;

public partial class PointsButton : ContentView
{
    [AutoBindable]
    private string _buttonText;
    
    [AutoBindable]
    private int _points;
    
	public PointsButton()
	{
		InitializeComponent();
	}
}