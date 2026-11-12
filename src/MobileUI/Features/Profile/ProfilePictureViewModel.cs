using Mopups.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.Mobile.Common;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class ProfilePictureViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly IPermissionsService _permissionsService;
    private readonly IAlertService _alertService;
    private readonly IApiErrorPresenter _errorPresenter;

    [ObservableProperty]
    private bool _useButtonEnabled;

    [ObservableProperty]
    private ImageSource _profilePicture = ImageSource.FromFile("");

    private FileResult _imageFile;


    [ObservableProperty]
    private bool _isUploading;


    public ProfilePictureViewModel(IUserService userService, IPermissionsService permissionsService, IAlertService alertService,
        IApiErrorPresenter errorPresenter)
    {
        _userService = userService;
        _permissionsService = permissionsService;
        _alertService = alertService;
        _errorPresenter = errorPresenter;
        userService.MyProfilePicObservable().Subscribe(myProfilePic => _profilePicture = myProfilePic);
    }

    [RelayCommand]
    private async Task TakePhoto()
    {
        // Permissions.StorageWrite gone and no longer needed on Android SDK 33 and above
        if (!OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            var storageGranted = await _permissionsService.CheckAndRequestPermission<Permissions.StorageWrite>();
            if (!storageGranted)
                return;
        }

        var cameraGranted = await _permissionsService.CheckAndRequestPermission<Permissions.Camera>();
        if (!cameraGranted)
            return;

        try
        {
            await CapturePhoto();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    [RelayCommand]
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
            await _alertService.DisplayAlertAsync("No Camera", "We cannot seem to access the Camera", "OK");
        }
    }

    [RelayCommand]
    private async Task UsePhoto()
    {
        if (!await _errorPresenter.EnsureOnlineAsync("change your profile picture"))
        {
            return;
        }

        IsUploading = true;
        var imageStream = await _imageFile.OpenReadAsync();

        try
        {
            await _userService.UploadImageAsync(imageStream, _imageFile.FileName);
            await MopupService.Instance.PopAllAsync();
        }
        catch (Exception e)
        {
            IsUploading = false;
            await _errorPresenter.HandleActionErrorAsync(e, "upload your picture");
        }
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
            await _alertService.DisplayAlertAsync("No Camera", "We cannot seem to access the Camera", "OK");
        }
    }

    [RelayCommand]
    private async Task ClosePage()
    {
        await MopupService.Instance.PopAsync();
    }
}