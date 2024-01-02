using Mopups.Pages;
using Mopups.Services;

namespace SSW.Rewards.PopupPages;

public partial class CameraPage
{
    public async void Handle_Tapped(object sender, EventArgs e)
    {
        await MopupService.Instance.PopAllAsync();
    }

    private CameraPageViewModel _viewModel { get; set; }

    public CameraPage(CameraPageViewModel cameraPageViewModel)
    {
        InitializeComponent();
        _viewModel = cameraPageViewModel;
        _viewModel.Navigation = Navigation;
        BindingContext = _viewModel;
    }
}
