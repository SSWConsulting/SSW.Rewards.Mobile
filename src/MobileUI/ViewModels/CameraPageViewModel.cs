using Mopups.Services;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class CameraPageViewModel : BaseViewModel
{
    public ICommand OnTakePhotoTapped { get; set; }
    public ICommand OnChoosePhotoTapped { get; set; }
    public ICommand UseButtonTapped { get; set; }

    [ObservableProperty]
    private bool _useButtonEnabled;

    [ObservableProperty]
    private ImageSource _profilePicture = ImageSource.FromFile("");
    
    private FileResult _imageFile;

    private IUserService _userService { get; set; }

    [ObservableProperty]
    private bool _isUploading;

    public CameraPageViewModel(IUserService userService)
    {

        OnTakePhotoTapped = new Command(async () => await Handle_takePhotoTapped());
        OnChoosePhotoTapped = new Command(async () => await Handle_choosePhotoTapped());
        UseButtonEnabled = false;

        UseButtonTapped = new Command(async () => await UploadProfilePic());

        _userService = userService;
    }

    public async Task Handle_takePhotoTapped()
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

            
            await CapturePhoto();
        }
        catch (Exception ex) 
        {
            Console.WriteLine(ex);
        }
    }
    
    public async Task Handle_choosePhotoTapped()
    {
        await ChoosePhoto();
    }

    private async Task SetPhoto(FileResult file)
    {
        if (file == null)
            return;

        _imageFile = file;

        // NOTE:    This is a workaround for the fact that the ImageSource.FromStream() method
        //          recursively closes streams all the way down.
        var tmpStream = new MemoryStream();
        
        using (var stream = await file.OpenReadAsync())
        {
            await stream.CopyToAsync(tmpStream);
        }

        var image = ImageSource.FromStream(() =>
        {
            var stream = new MemoryStream(tmpStream.ToArray());
            return stream;
        });

        ProfilePicture = image;

        UseButtonEnabled = true;
    }

    private async Task CapturePhoto()
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Take a photo"
            });

            if (photo is not null)
            {
                await SetPhoto(photo);
            }
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("No Camera", "We cannot seem to access the Camera", "OK");
        }
    }

    private async Task ChoosePhoto()
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Choose a photo"
            });

            if (photo is not null)
            {
                await SetPhoto(photo);
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
        var imageStream = await _imageFile.OpenReadAsync();
        await _userService.UploadImageAsync(imageStream, _imageFile.FileName);
        await MopupService.Instance.PopAllAsync();
    }
}