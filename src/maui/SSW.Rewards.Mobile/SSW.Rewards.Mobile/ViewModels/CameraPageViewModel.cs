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

    private byte[] stream;

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

    public async void Handle_takePhotoTapped()
    {
        PermissionStatus storageStatus = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
        PermissionStatus cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();

        try
        {

            if (storageStatus != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.StorageWrite>();
            }


            if (cameraStatus != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.Camera>();
            }


            CapturePhoto();
        }
        catch (Exception ex) 
        {
            Console.WriteLine(ex);
        }
    }
    public void Handle_choosePhotoTapped()
    {
        ChoosePhoto();
    }

    private async Task SetPhoto(Stream stream)
    {
        if (stream == null)
            return;

        var memStream = new MemoryStream();
        stream.CopyTo(memStream);
        this.stream = memStream.ToArray();

        var image = ImageSource.FromStream(() => stream);

        ProfilePicture = image;
        UseButtonEnabled = true;
        RaisePropertyChanged("UseButtonEnabled");
        //RaisePropertyChanged("ProfilePicture", "UseButtonEnabled");
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
                await SetPhoto(sourceStream);
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
                await SetPhoto(sourceStream);
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
        Stream dstStream = new MemoryStream();
        dstStream.Write(this.stream,0, this.stream.Length);
        await _userService.UploadImageAsync(dstStream);
        await MopupService.Instance.PopAllAsync();
    }
}