using Mopups.Pages;
using Mopups.Services;

namespace SSW.Rewards.PopupPages;

public partial class CameraPage
{
    public CameraPage(CameraPageViewModel cameraPageViewModel)
    {
        InitializeComponent();
        cameraPageViewModel.Navigation = Navigation;
        BindingContext = cameraPageViewModel;
    }
}
