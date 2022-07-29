using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Infrastructure;

public class ProfileStorageProvider : IProfileStorageProvider
{
    private readonly IStorageProvider _storageProvider;

    private const string CONTAINER_NAME = "profile";

    public ProfileStorageProvider(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    public async Task<byte[]> GetProfileData() => await _storageProvider.DownloadBlob(CONTAINER_NAME, "NDC-Profiles-2019.xlsx");

    public async Task<Uri> GetProfileUri(string staffMemberName) => await _storageProvider.GetUri(CONTAINER_NAME, $"{staffMemberName.ToLower()}.png");

    public async Task<string> UploadProfilePicture(byte[] imageArray, string fileName)
    {
        await _storageProvider.UploadBlob(CONTAINER_NAME, fileName, imageArray);

        var uri = await _storageProvider.GetUri(CONTAINER_NAME, fileName);

        return uri.AbsoluteUri;
    }
}
