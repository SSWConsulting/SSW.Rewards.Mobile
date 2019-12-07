
using System.Windows.Input;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace SSW.Rewards.ViewModels
{
    public class CameraPageViewModel : BaseViewModel
    {
        public ICommand OnTakePhotoTapped { get; set; }
        public ImageSource ProfilePicture { get; set; } = ImageSource.FromFile("");
        public Page page;

        public CameraPageViewModel()
        {
      
            OnTakePhotoTapped = new Command(Handle_takePhotoClicked);
        }

        public void Handle_takePhotoClicked()
        {
            CapturePhoto();
        }

         private async void CapturePhoto()
        {
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await page.DisplayAlert("No Camera", "We cannot seem to access the Camera", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                Directory = "Temp",
                Name = "profile.jpg",
                PhotoSize = PhotoSize.Small
                
            });

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
    }
}
