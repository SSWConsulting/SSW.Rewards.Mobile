
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace SSW.Rewards.ViewModels
{
    public class CameraPageViewModel : BaseViewModel
    {
        public ICommand OnTakePhotoTapped { get; set; }
        public ICommand OnChoosePhotoTapped { get; set; }

        public ImageSource ProfilePicture { get; set; } = ImageSource.FromFile("");
        public Page page;

        public CameraPageViewModel()
        {

            OnTakePhotoTapped = new Command(Handle_takePhotoTapped);
            OnChoosePhotoTapped = new Command(Handle_choosePhotoTapped);

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

            var image = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                file.Dispose();
                return stream;
            });

            ProfilePicture = image;
            RaisePropertyChanged("ProfilePicture");
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
                    DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Front
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
                await page.DisplayAlert("No Camera", "We cannot seem to access your Photos", "OK");
            }
        }
    }
}
