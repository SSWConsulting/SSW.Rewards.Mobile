using Mopups.Services;
using System.Windows.Input;

namespace SSW.Rewards.ViewModels;

public class CameraPageViewModel : BaseViewModel
{
    public ICommand OnTakePhotoTapped { get; set; }
    public ICommand OnChoosePhotoTapped { get; set; }
    public ICommand UseButtonTapped { get; set; }

    public bool UseButtonEnabled { get; set; }

    public ImageSource ProfilePicture { get; set; } = ImageSource.FromFile("");

    private Stream stream;

    private IUserService _userService { get; set; }

    public bool IsUploading { get; set; } = false;

    public CameraPageViewModel(IUserService userService)
    {

        OnTakePhotoTapped = new Command(Handle_takePhotoTapped);
        OnChoosePhotoTapped = new Command(Handle_choosePhotoTapped);
        UseButtonEnabled = false;

        UseButtonTapped = new Command(async () => await UploadProfilePic());

        _userService = userService;

    }

    public void Handle_takePhotoTapped()
    {
        CapturePhoto();
    }
    public void Handle_choosePhotoTapped()
    {
        ChoosePhoto();
    }

    private void SetPhoto(Stream stream)
    {
        if (stream == null)
            return;

        this.stream = stream;

        var image = ImageSource.FromStream(() => stream);

        ProfilePicture = image;
        UseButtonEnabled = true;
        RaisePropertyChanged("ProfilePicture", "UseButtonEnabled");
    }


    private async void CapturePhoto()
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Take a photo"
            });

            if (photo is not null)
            {
                using Stream sourceStream = await photo.OpenReadAsync();
                SetPhoto(sourceStream);
            }
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("No Camera", "We cannot seem to access the Camera", "OK");
        }
    }

    private async void ChoosePhoto()
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Choose a photo"
            });

            if (photo is not null)
            {
                using Stream sourceStream = await photo.OpenReadAsync();
                SetPhoto(sourceStream);
            }
        }
        else
        {
            await App.Current.MainPage.DisplayAlert("No Camera", "We cannot seem to access the Camera", "OK");
        }
    }

    public async Task UploadProfilePic()
    {
        IsUploading = true;
        RaisePropertyChanged("IsUploading");
        await _userService.UploadImageAsync(stream);
        await MopupService.Instance.PopAllAsync();
    }
}