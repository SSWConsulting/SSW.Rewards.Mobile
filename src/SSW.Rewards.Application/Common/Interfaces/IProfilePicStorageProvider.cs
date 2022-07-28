namespace SSW.Rewards.Application.Common.Interfaces;
public interface IProfilePicStorageProvider
{
    Task<string> UploadProfilePic(byte[] imageArray, string fileName);
    Task<Uri> GetProfilePicUri(string picId);
}
