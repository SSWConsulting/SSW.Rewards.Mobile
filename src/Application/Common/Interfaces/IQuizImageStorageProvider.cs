namespace SSW.Rewards.Application.Common.Interfaces;

public interface IQuizImageStorageProvider
{
    Task<byte[]> GetBlob(string blobName);
    Task<Uri> GetQuizUri(string staffMemberName);
    Task<string> UploadCarouselImage(byte[] imageArray, string fileName);
}