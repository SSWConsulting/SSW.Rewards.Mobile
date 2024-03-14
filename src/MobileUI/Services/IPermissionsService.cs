namespace SSW.Rewards.Mobile.Services;

public interface IPermissionsService
{
    Task<bool> CheckAndRequestPermission<TPermission>() where TPermission : Permissions.BasePermission, new();
}