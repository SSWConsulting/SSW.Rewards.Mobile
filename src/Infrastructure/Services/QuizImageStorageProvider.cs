using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Infrastructure.Services;

public class QuizImageStorageProvider : IQuizImageStorageProvider
{
    private readonly IStorageProvider _storageProvider;

    private const string CONTAINER_NAME = "quizzes";

    public QuizImageStorageProvider(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    public async Task<byte[]> GetBlob(string blobName) => await _storageProvider.DownloadBlob(CONTAINER_NAME, blobName);

    public async Task<Uri> GetQuizUri(string quizName) => await _storageProvider.GetUri(CONTAINER_NAME, $"{quizName.ToLower()}.png");

    public async Task<string> UploadCarouselImage(byte[] imageArray, string fileName)
    {
        await _storageProvider.UploadBlob(CONTAINER_NAME, fileName, imageArray);

        var uri = await _storageProvider.GetUri(CONTAINER_NAME, fileName);

        return uri.AbsoluteUri;
    }
}
