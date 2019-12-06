using System;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using SSW.Rewards.ViewModels;
using Xamarin.Forms;

namespace SSW.Rewards.PopupPages
{
    public partial class CameraPage : PopupPage
    {
        public async void Handle_Tapped(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAllAsync();
        }

        async void Handle_takePhotoClicked(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera avaialble.", "OK");
                return;
            }
            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                Directory = "Sample",
                Name = "test.jpg"
            });
            if (file == null)
                return;
            await DisplayAlert("File Location", file.Path, "OK");
            image.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                file.Dispose();
                return stream;
            });
        }

        private CameraPageViewModel _viewModel { get; set; }

        public CameraPage()
        {
            InitializeComponent();
            _viewModel = Resolver.Resolve<CameraPageViewModel>();
            _viewModel.Navigation = Navigation;
            BindingContext = _viewModel;
        }
    }
}
