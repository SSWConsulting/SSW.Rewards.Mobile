using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Infrastructure.Services;

public class SkillPicStorageProvider : ISkillPicStorageProvider
{
    private readonly IStorageProvider _storageProvider;
    private const string CONTAINER_NAME = "skillpics";

    public SkillPicStorageProvider(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    public async Task<Uri> GetSkillPicUri(string picId) => await _storageProvider.GetUri(CONTAINER_NAME, picId);

    public async Task<Uri> UploadSkillPic(byte[] imageArray, string filename)
    {
        string id = Guid.NewGuid().ToString() + filename;
        await _storageProvider.UploadBlob(CONTAINER_NAME, id, imageArray);

        var uri = await _storageProvider.GetUri(CONTAINER_NAME, id);

        return uri;
    }
} 