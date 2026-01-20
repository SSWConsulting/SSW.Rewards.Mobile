using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Infrastructure;

public class PostImageStorageProvider : IPostImageStorageProvider
{
    private readonly IStorageProvider _storageProvider;

    private const string CONTAINER_NAME = "postimages";

    public PostImageStorageProvider(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    public async Task<string> UploadPostImage(byte[] imageArray, string fileName)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ArgumentException("File name must have a valid extension", nameof(fileName));
        }

        string id = $"{Guid.NewGuid()}{extension}";
        await _storageProvider.UploadBlob(CONTAINER_NAME, id, imageArray);

        var uri = await _storageProvider.GetUri(CONTAINER_NAME, id);

        return uri.AbsoluteUri;
    }

    public async Task<Uri> GetPostImageUri(string picId) => await _storageProvider.GetUri(CONTAINER_NAME, picId);
}
