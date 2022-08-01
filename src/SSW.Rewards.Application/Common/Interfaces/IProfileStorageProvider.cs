namespace SSW.Rewards.Application.Common.Interfaces;

public interface IProfileStorageProvider
{
    Task<byte[]> GetBlob(string blobName);
    Task<Uri> GetProfileUri(string staffMemberName);
    Task<string> UploadProfilePicture(byte[] imageArray, string fileName);
}
