namespace SSW.Rewards.Application.Common.Interfaces;

public interface IPostImageStorageProvider
{
    Task<string> UploadPostImage(byte[] imageArray, string fileName);
}
