using Maui.BindableProperty.Generator.Core;

namespace SSW.Rewards.Mobile.Controls;

public partial class FlyoutHeader : ContentView
{
    [AutoBindable]
    private string profilePic;

    [AutoBindable]
    private string name;

    [AutoBindable]
    private string email;

    [AutoBindable]
    private bool staff;

    public FlyoutHeader()
    {
        InitializeComponent();

        BindingContext = this;
    }
}
