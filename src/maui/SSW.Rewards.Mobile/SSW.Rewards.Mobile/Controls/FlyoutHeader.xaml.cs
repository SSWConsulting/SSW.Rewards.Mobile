namespace SSW.Rewards.Controls;

public partial class FlyoutHeader : ContentView
{
    public FlyoutHeader(FlyoutHeaderViewModel viewModel)
    {
        InitializeComponent();
        viewModel.Navigation = Navigation;
        BindingContext = viewModel;
    }

    public FlyoutHeader()
    {
        InitializeComponent();
        var viewModel = Resolver.Resolve<FlyoutHeaderViewModel>();
        viewModel.Navigation = Navigation;
        BindingContext = viewModel;
    }
}
