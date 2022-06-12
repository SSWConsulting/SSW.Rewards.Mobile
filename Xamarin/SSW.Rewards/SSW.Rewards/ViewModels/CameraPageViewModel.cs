
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Rg.Plugins.Popup.Services;
using SSW.Rewards.Services;
using Xamarin.Forms;

namespace SSW.Rewards.ViewModels
{
    public class CameraPageViewModel : BaseViewModel
    {
        public ICommand OnTakePhotoTapped { get; set; }
        public ICommand OnChoosePhotoTapped { get; set; }
        public ICommand UseButtonTapped { get; set; }

        public bool UseButtonEnabled { get; set; }

        public ImageSource ProfilePicture { get; set; } = ImageSource.FromFile("");

        private MediaFile fileStream { get; set; }
        public Page page;

        private IUserService _userService { get; set; }

        public bool IsUploading { get; set; } = false;

        public CameraPageViewModel()
        {

            OnTakePhotoTapped = new Command(Handle_takePhotoTapped);
            OnChoosePhotoTapped = new Command(Handle_choosePhotoTapped);
            UseButtonEnabled = false;

            UseButtonTapped = new Command(async () => await UploadProfilePic());

            _userService = Resolver.Resolve<IUserService>();

        }

        public void Handle_takePhotoTapped()
        {
            CapturePhoto();
        }
        public void Handle_choosePhotoTapped()
        {
            ChoosePhoto();
        }

        private void SetPhoto(MediaFile file)
        {
            if (file == null)
                return;

            fileStream = file;

            var image = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                //file.Dispose();
                return stream;
            });

            ProfilePicture = image;
            UseButtonEnabled = true;
            RaisePropertyChanged("ProfilePicture", "UseButtonEnabled");
        }


        private async void CapturePhoto()
        {
            try
            {
                var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    Directory = "Temp",
                    Name = "profile.jpg",
                    PhotoSize = PhotoSize.Small,
                    DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Front,

                    AllowCropping = true
                });

                SetPhoto(file);
            }
            catch (MediaPermissionException)
            {
                await page.DisplayAlert("No Camera", "We cannot seem to access the Camera", "OK");
            }
        }

        private async void ChoosePhoto()
        {
            try
            {
                var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                {
                    PhotoSize = PhotoSize.Small,
                    SaveMetaData = false
                });

                SetPhoto(file);
            }
            catch (MediaPermissionException)
            {
                await page.DisplayAlert("No Photos", "We cannot seem to access your Photos", "OK");
            }
        }

        public async Task UploadProfilePic()
        {
            IsUploading = true;
            RaisePropertyChanged("IsUploading");
            await _userService.UploadImageAsync(fileStream.GetStream());
            await PopupNavigation.Instance.PopAllAsync();
        }
    }
}
