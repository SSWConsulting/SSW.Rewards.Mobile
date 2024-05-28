
namespace SSW.Rewards.Mobile.Services;

public interface IPermissionsService
{
    Task<bool> CheckAndRequestPermission<TPermission>() where TPermission : Permissions.BasePermission, new();
}

public class PermissionsService : IPermissionsService
{
    public async Task<bool> CheckAndRequestPermission<TPermission>() where TPermission : Permissions.BasePermission, new()
    {
        bool result;
#if ANDROID
        result = await CheckAndRequestPermissionOnAndroid<TPermission>();
#elif IOS
        result = await CheckAndRequestPermissionOnIOS<TPermission>();
#endif

        return result;
    }

    // ReSharper disable once UnusedMember.Local
    private async Task<bool> CheckAndRequestPermissionOnAndroid<TPermission>() where TPermission : Permissions.BasePermission, new()
    {
        var status = await Permissions.CheckStatusAsync<TPermission>(); // for the first check returns PermissionStatus.Denied
        if (status == PermissionStatus.Granted)
        {
            return true;
        }

        return await RequestPermission<TPermission>();
    }

    // ReSharper disable once UnusedMember.Local
    private async Task<bool> CheckAndRequestPermissionOnIOS<TPermission>() where TPermission : Permissions.BasePermission, new()
    {
        var status = await Permissions.CheckStatusAsync<TPermission>(); // for the first check returns PermissionStatus.Unknown
        if (status == PermissionStatus.Granted)
        {
            return true;
        }

        if (status == PermissionStatus.Denied)
        {
            // Prompt the user to turn on in settings
            // On iOS once a permission has been denied it may not be requested again from the application
            var message =
                $"You need to grant access to {typeof(TPermission).Name} in Settings on your phone to use this feature.";
            await CommunityToolkit.Maui.Alerts.Snackbar
                .Make(message, duration: TimeSpan.FromSeconds(5))
                .Show();

            return false;
        }

        return await RequestPermission<TPermission>();
    }

    private static async Task<bool> RequestPermission<TPermission>() where TPermission : Permissions.BasePermission, new()
    {
        var requestStatus = await Permissions.RequestAsync<TPermission>();
        if (requestStatus == PermissionStatus.Granted)
        {
            return true;
        }

        await CommunityToolkit.Maui.Alerts.Snackbar
            .Make("You cannot use this feature without granting access.", duration: TimeSpan.FromSeconds(5))
            .Show();

        return false;
    }
}