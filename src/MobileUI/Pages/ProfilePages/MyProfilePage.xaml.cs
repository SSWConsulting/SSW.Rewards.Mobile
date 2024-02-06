using SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

namespace SSW.Rewards.Mobile.Pages;

public partial class MyProfilePage : ContentPage
{
    private bool _initialised;

    private MyProfileViewModel viewModel;

    private bool _bottomRowAdded = false;


    public MyProfilePage(MyProfileViewModel vm)
    {
        InitializeComponent();
        viewModel = vm;
        viewModel.Navigation = Navigation;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        if (!_initialised)
            await viewModel.Initialise();

        _initialised = true;
        
        viewModel.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        viewModel.OnDisappearing();
    }

    #region Technical Debt
    // TECH DEBT: The following workaround adds an extra row on iOS.
    //            This is necessary due to a bug in .NET MAUI Preview that
    //            causes part of the page to be hidden behind the tab bar.
    //            See: https://github.com/dotnet/maui/issues/17817

    private void AddBottomPadding()
    {
        var paddingHeight = GetBottomRowPadding();

        var rowHeight = new GridLength(paddingHeight);

        MainPageRow.RowDefinitions.Add(new RowDefinition(rowHeight));
    }

    private double GetBottomRowPadding()
    {
        var screenHeightInPixels = DeviceDisplay.MainDisplayInfo.Height;
        var screenHeight = screenHeightInPixels / DeviceDisplay.MainDisplayInfo.Density;

        var bottom = this.Bounds.Bottom;

        var crudeBarHeight = screenHeight - bottom;

        return crudeBarHeight;
    }


    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (DeviceInfo.Platform == DevicePlatform.iOS && !_bottomRowAdded)
        {
            _bottomRowAdded = true;
            AddBottomPadding();
        }
    }
    #endregion
}